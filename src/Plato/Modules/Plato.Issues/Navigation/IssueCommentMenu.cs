﻿using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Plato.Issues.Models;
using Plato.Entities.Extensions;
using Plato.Internal.Models.Users;
using Plato.Internal.Navigation.Abstractions;
using Plato.Internal.Security.Abstractions;

namespace Plato.Issues.Navigation
{

    public class IssueCommentMenu : INavigationProvider
    {

        public IStringLocalizer T { get; set; }

        public IssueCommentMenu(IStringLocalizer localizer)
        {
            T = localizer;
        }

        public void BuildNavigation(string name, INavigationBuilder builder)
        {

            if (!String.Equals(name, "issue-comment", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Get entity from context
            var entity = builder.ActionContext.HttpContext.Items[typeof(Issue)] as Issue;
            if (entity == null)
            {
                return;
            }

            // Get reply from context
            var reply = builder.ActionContext.HttpContext.Items[typeof(Comment)] as Comment;
            if (reply == null)
            {
                return;
            }

            //// Get authenticated user from features
            var user = builder.ActionContext.HttpContext.Features[typeof(User)] as User;

            // Get delete / restore permission
            Permission deletePermission = null;
            if (reply.IsDeleted)
            {
                deletePermission = user?.Id == reply.CreatedUserId
                    ? Permissions.RestoreOwnIssueComments
                    : Permissions.RestoreAnyIssueComment;
            }
            else
            {
                deletePermission = user?.Id == reply.CreatedUserId
                    ? Permissions.DeleteOwnIssueComments
                    : Permissions.DeleteAnyIssueComment;
            }

            // Options
            builder
                .Add(T["Options"], int.MaxValue, options => options
                        .IconCss("fa fa-ellipsis-h")
                        .Attributes(new Dictionary<string, object>()
                        {
                            {"data-provide", "tooltip"},
                            {"title", T["Options"]}
                        })
                        .Add(T["Edit"], int.MinValue, edit => edit
                            .Action("EditReply", "Home", "Plato.Issues", new RouteValueDictionary()
                            {
                                ["id"] = reply?.Id ?? 0
                            })
                            .Permission(user?.Id == reply.CreatedUserId
                                ? Permissions.EditOwnIssueComments
                                : Permissions.EditAnyIssueComment)
                            .LocalNav())
                        .Add(reply.IsHidden ? T["Unhide"] : T["Hide"], 2, edit => edit
                            .Action(reply.IsHidden ? "ShowReply" : "HideReply", "Home", "Plato.Issues",
                                new RouteValueDictionary()
                                {
                                    ["id"] = reply?.Id ?? 0
                                })
                            .Resource(entity.CategoryId)
                            .Permission(reply.IsHidden
                                ? Permissions.ShowIssueComments
                                : Permissions.HideIssueComments)
                            .LocalNav()
                        )
                        .Add(reply.IsSpam ? T["Not Spam"] : T["Spam"], 3, spam => spam
                            .Action(reply.IsSpam ? "ReplyFromSpam" : "ReplyToSpam", "Home", "Plato.Issues",
                                new RouteValueDictionary()
                                {
                                    ["id"] = reply?.Id ?? 0
                                })
                            .Resource(entity.CategoryId)
                            .Permission(reply.IsSpam
                                ? Permissions.IssueCommentFromSpam
                                : Permissions.IssueCommentToSpam)
                            .LocalNav()
                        )
                        .Add(T["Report"], int.MaxValue - 2, report => report
                            .Action("Report", "Home", "Plato.Issues", new RouteValueDictionary()
                            {
                                ["opts.id"] = entity.Id,
                                ["opts.alias"] = entity.Alias,
                                ["opts.replyId"] = reply.Id
                            })
                            .Attributes(new Dictionary<string, object>()
                            {
                                {"data-provide", "dialog"},
                                {"data-dialog-modal-css", "modal fade"},
                                {"data-dialog-css", "modal-dialog modal-lg"}
                            })
                            .Permission(Permissions.ReportIssueComments)
                            .LocalNav()
                        )
                        .Add(T["Divider"], int.MaxValue - 1, divider => divider
                            .Permission(deletePermission)
                            .DividerCss("dropdown-divider").LocalNav()
                        )
                        .Add(reply.IsDeleted ? T["Restore"] : T["Delete"], int.MaxValue, edit => edit
                                .Action(reply.IsDeleted ? "RestoreReply" : "DeleteReply", "Home", "Plato.Issues",
                                    new RouteValueDictionary()
                                    {
                                        ["id"] = reply.Id
                                    })
                                .Permission(deletePermission)
                                .LocalNav(),
                            reply.IsDeleted
                                ? new List<string>() {"dropdown-item", "dropdown-item-success"}
                                : new List<string>() {"dropdown-item", "dropdown-item-danger"}
                        )
                    , new List<string>() {"article-options", "text-muted", "dropdown-toggle-no-caret", "text-hidden"}
                );

            // If the reply if deleted display permanent delete option
            if (reply.IsDeleted)
            {

                // Permanent delete permissions
                var permanentDeletePermission = reply.CreatedUserId == user?.Id
                    ? Permissions.PermanentDeleteOwnIssueComments
                    : Permissions.PermanentDeleteAnyIssueComment;

                builder
                    .Add(T["Delete"], int.MinValue, options => options
                            .IconCss("fal fa-trash-alt")
                            .Attributes(new Dictionary<string, object>()
                            {
                                {"data-toggle", "tooltip"},
                                {"data-provide", "confirm"},
                                {"title", T["Permanent Delete"]},
                            })
                            .Action("PermanentDeleteReply", "Home", "Plato.Issues",
                                new RouteValueDictionary()
                                {
                                    ["id"] = reply.Id
                                })
                            .Permission(permanentDeletePermission)
                            .LocalNav()
                        , new List<string>() { "issue-permanent-delete", "text-muted", "text-hidden" }
                    );
            }

            // If entity & reply are not hidden and entity is not locked allow replies
            if (!entity.IsHidden() && !reply.IsHidden() && !entity.IsLocked)
            {

                builder
                    .Add(T["Comment"], int.MaxValue, options => options
                            .IconCss("fa fa-reply")
                            .Attributes(new Dictionary<string, object>()
                            {
                                {"data-provide", "postQuote"},
                                {"data-quote-selector", "#quote" + reply.Id.ToString()},
                                {"data-toggle", "tooltip"},
                                {"title", T["Comment"]}
                            })
                            .Action("Login", "Account", "Plato.Users",
                                new RouteValueDictionary()
                                {
                                    ["returnUrl"] = builder.ActionContext.HttpContext.Request.Path
                                })
                            .Permission(Permissions.PostIssueComments)
                            .LocalNav()
                        , new List<string>() {"article-reply", "text-muted", "text-hidden"}
                    );

            }
            
        }

    }

}












