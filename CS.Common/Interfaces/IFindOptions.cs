namespace CS.Common.Interfaces
{
    public interface IFindOptions
    {
        /// <summary>
        /// Fields to return populated
        /// </summary>
        string[] ReturnFields { get; set; }

        /// <summary>
        /// set number of records that will be fetched each time database is hit
        /// </summary>
        int BatchSize { get; set; }

        /// <summary>
        /// allow querying to be done on read-only replication server. If true, data could be stale
        /// </summary>
        bool SlaveOK { get; set; }

        /// <summary>
        /// force server to return all records at once.
        /// </summary>
        bool Exhaust { get; set; }

        /// <summary>
        /// Allow query to timeout.  (Default is false)
        /// </summary>
        bool AllowTimeout { get; set; }

        /// <summary>
        /// Maximum number of records that can be returned (Default '0' = unlimited)
        /// </summary>
        int RecordLimit { get; set; }

        /// <summary>
        /// Hint on which index should be used to find data (MongoDB)
        /// </summary>
        string HintIndexName { get; set; }

        /// <summary>
        /// Number of records to skip.  (To be used when paging through data, and to be used with RecordLimit)
        /// </summary>
        int Skip { get; set; }

        /// <summary>
        /// Sort results ascending by these fields
        /// </summary>
        string[] SortFields { get; set; }

        bool RetrieveTracingInformation { get; set; }

    }
}