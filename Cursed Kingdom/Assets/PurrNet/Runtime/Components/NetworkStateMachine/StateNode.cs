namespace PurrNet.StateMachine
{
    public abstract class StateNode : NetworkBehaviour
    {
        protected StateMachine machine { get; private set; }

        public void Setup(StateMachine stateMachine)
        {
            machine = stateMachine;
        }
        
        public virtual void Enter(bool asServer) {}
        
        public virtual void StateUpdate(bool asServer) {}
        
        public virtual void Exit(bool asServer) {}
    }
    
    public abstract class StateNode<T> : StateNode
    {
        public virtual void Enter(T data, bool asServer) {}
        
        public virtual void Resume(T data, bool asServer) {}
    }
}