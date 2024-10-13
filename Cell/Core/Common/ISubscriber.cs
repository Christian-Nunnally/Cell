namespace Cell.Core.Execution
{
    /// <summary>
    /// An object can that subscribe to some noficiation mechanism, which can invoke the action on this subscriber.
    /// </summary>
    public interface ISubscriber
    {
        /// <summary>
        /// The action that is performed when this ISubscriber triggers.
        /// </summary>
        void Action();
    }
}
