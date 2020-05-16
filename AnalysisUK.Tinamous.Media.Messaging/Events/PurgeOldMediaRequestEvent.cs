using System;
using AnalysisUK.Tinamous.Messaging.Common.Dtos;

namespace AnalysisUK.Tinamous.Media.Messaging.Events
{
    /// <summary>
    /// Event style request from the scheduling service to purge old media.
    /// </summary>
    public class PurgeOldMediaRequestEvent
    {
        /// <summary>
        /// Purge all media upto this date.
        /// </summary>
        /// <remarks>
        /// Expect the scheduler service to know of the member media retention policy and to apply that.
        /// </remarks>
        public DateTime UpTo { get; set; }

        /// <summary>
        /// The user to purge the media of.
        /// </summary>
        public UserSummaryDto User { get; set; }

        /// <summary>
        /// How many attempts it's taken so far to try and delete the
        /// media. prevents constant attempts until the end of time if
        /// something has gone wrong.
        /// </summary>
        public int Attempt { get; set; }
    }
}