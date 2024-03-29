﻿using API.Services;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Batch;
using Microsoft.AspNetCore.OData.Extensions;

namespace API.Classes {
    public class MyODataBatchHandler : DefaultODataBatchHandler {

        private readonly IMessageService _messageService;

        public MyODataBatchHandler(IMessageService messageService) : base() {
            _messageService = messageService;
        }

        public static Guid? GetChangeSetId(HttpRequest request) {
            return ((ODataBatchFeature?)request.HttpContext.Features[typeof(IODataBatchFeature)])?.ChangeSetId;
        }

        public override async Task<IList<ODataBatchRequestItem>> ParseBatchRequestsAsync(HttpContext context) {
            var requests = await base.ParseBatchRequestsAsync(context);
            return requests.Select(rq => {
                if (rq is ChangeSetRequestItem) {
                    return new TransactionalChangesetRequestItem(rq as ChangeSetRequestItem, _messageService);
                } else {
                    return rq;
                }
            }).ToList();
        }

        public class TransactionalChangesetRequestItem : ODataBatchRequestItem {
            private ChangeSetRequestItem _changeSetRequestItem;
            private readonly IMessageService _messageService;

            public TransactionalChangesetRequestItem(ChangeSetRequestItem changeSetRequestItem, IMessageService messageService) : base() {
                _changeSetRequestItem = changeSetRequestItem;
                _messageService = messageService;
            }

            public override async Task<ODataBatchResponseItem> SendRequestAsync(RequestDelegate handler) {
                var response = await _changeSetRequestItem.SendRequestAsync(handler) as ChangeSetResponseItem;
                var changeSetId = GetChangeSetId(_changeSetRequestItem.Contexts.First().Request);
                if (response != null && response.Contexts.All(c => c.Response.IsSuccessStatusCode())) {
                    // Write all events and commit changes to DB if successful
                    Console.WriteLine("Sending all events");
                    try {
                        _messageService.SendDelayed(changeSetId);
                        //transaction.Commit();
                    } catch (Exception ex) {
                        //transaction.Rollback();
                        _messageService.ClearDelayed(changeSetId);
                        throw;
                    }
                } else {
                    //transaction.Rollback();
                    _messageService.ClearDelayed(changeSetId);
                }
                return response;
            }
        }
    }
}
