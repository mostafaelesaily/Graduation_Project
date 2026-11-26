using System;
using System.Collections.Generic;
using System.Text;
using NotificationEntity = MedLink.Domain.Entities.Common.Notifications;

namespace MedLink.Application.Specifications.Notifications
{
    public class NotificationsByUserSpecification : BaseSpecification<NotificationEntity>
    {
        // Paged query: notifications belonging to a user, optionally unread-only,
        // ordered newest-first with a stable secondary ordering.
        public NotificationsByUserSpecification
            (
             string userId ,
             bool unreadOnly ,
             int pageNum ,
             int pageSize
            ) : base(n =>
             n.UserId == userId &&
             (!unreadOnly || !n.IsRead)
            )
        {
            AddOrderByDescending(n => n.CreatedAt);
            AddThenByDescending(n => n.Id);

            ApplyPaging(
            (pageNum - 1) * pageSize,
            pageSize);
        }

        // Count-only query: same filtering criteria, no ordering/paging
        // (used for TotalCount and GetUnreadCountAsync).
        public NotificationsByUserSpecification
            (
             string userId ,
             bool unreadOnly
            ) : base(n =>
             n.UserId == userId &&
             (!unreadOnly || !n.IsRead)
            )
        {
        }
    }
}
