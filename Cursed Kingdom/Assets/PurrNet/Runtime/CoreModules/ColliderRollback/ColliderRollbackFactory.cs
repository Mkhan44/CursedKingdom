namespace PurrNet.Modules
{
    public class ColliderRollbackFactory : SceneScopedFactory<RollbackModule>, IPostFixedUpdate
    {
        readonly TickManager _tick;

        public ColliderRollbackFactory(TickManager tick, ScenesModule scenes)
            : base(scenes)
        {
            _tick = tick;
        }

        protected override RollbackModule CreateModule(SceneID scene, bool asServer)
        {
            if (scenes.TryGetSceneState(scene, out var state))
                return new RollbackModule(_tick, state.scene);
            return new RollbackModule(_tick, default);
        }

        public void PostFixedUpdate()
        {
            foreach (var module in modules)
                module.OnPostTick();
        }
    }
}