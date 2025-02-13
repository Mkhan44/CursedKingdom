using System;
using System.Collections.Generic;
using PurrNet.Utils;
using UnityEngine;
using UnityEngine.Playables;

namespace PurrNet
{
    public sealed partial class NetworkAnimator : NetworkIdentity
    {
        [PurrDocs("systems-and-modules/plug-n-play-components/network-animator")]
        [Tooltip("The animator to sync")]
        [SerializeField, PurrLock] private Animator _animator;
        [Tooltip("If true the owner has authority over this animator, if no owner is set it is controlled by the server")]
        [SerializeField, PurrLock] private bool _ownerAuth = true;
        [Tooltip("Automatically sync parameters when they are changed, if you are using NetworkAnimator directly you should set this to false")]
        [SerializeField, PurrLock] private bool _autoSyncParameters = true;
        [SerializeField]
        [Tooltip("Parameters names that should not be synced")]
        private List<string> _dontSyncParameters = new List<string>();

        readonly HashSet<int> _sontSyncHashes = new HashSet<int>();
        
        /// <summary>
        /// If true the owner has authority over this animator, if no owner is set it is controlled by the server
        /// </summary>
        public bool ownerAuth => _ownerAuth;
        
        /// <summary>
        /// Automatically sync parameters when they are changed,
        /// if you are using NetworkAnimator directly you should set this to false
        /// </summary>
        public bool autoSyncParameters => _autoSyncParameters;
        
        /// <summary>
        /// The animator to sync
        /// </summary>
        public Animator animator => _animator;
        
        public List<string> dontSyncParameters => _dontSyncParameters;

        private void Awake()
        {
            foreach (var param in _dontSyncParameters)
                _sontSyncHashes.Add(Animator.StringToHash(param));
        }

        void OnEnable()
        {
            if (_animator && !_animator.enabled)
                _animator.enabled = true;
        }

        void OnDisable()
        {
            if (_animator && _animator.enabled)
                _animator.enabled = false;
        }

        private void Reset()
        {
            _animator = GetComponent<Animator>();
        }
        
        private void IfSameTypeReplace(NetAnimatorRPC action)
        {
            if (_dirty.Count > 0 && _dirty[^1].type == action.type)
            {
                _dirty[^1] = action;
            }
            else
            {
                _dirty.Add(action);
            }
        }
        
        private void IfSameReplace(NetAnimatorRPC action, Func<NetAnimatorRPC, NetAnimatorRPC, bool> predicate)
        {
            if (_dirty.Count > 0 && _dirty[^1].type == action.type)
            {
                var other = _dirty[^1];
                if (predicate(action, other))
                    _dirty[^1] = action;
                else _dirty.Add(action);
            }
            else
            {
                _dirty.Add(action);
            }
        }
        
        public AnimatorControllerParameter[] parameters => _animator.parameters;
        
        public RuntimeAnimatorController runtimeAnimatorController => _animator.runtimeAnimatorController;
        
        public float leftFeetBottomHeight => _animator.leftFeetBottomHeight;
        
        public float rightFeetBottomHeight => _animator.rightFeetBottomHeight;
        
        public Vector3 velocity => _animator.velocity;
        
        public Vector3 angularVelocity => _animator.angularVelocity;
        
        public bool isInitialized => _animator.isInitialized;
        
        public Transform avatarRoot => _animator.avatarRoot;
        
        public Vector3 deltaPosition => _animator.deltaPosition;
        
        public Quaternion deltaRotation => _animator.deltaRotation;
        
        public float gravityWeight => _animator.gravityWeight;
        
        public float humanScale => _animator.humanScale;
        
        public bool isHuman => _animator.isHuman;
        
        public bool isOptimizable => _animator.isOptimizable;
        
        public int layerCount => _animator.layerCount;
        
        public int parameterCount => _animator.parameterCount;
        
        public Vector3 pivotPosition => _animator.pivotPosition;
        
        public float pivotWeight => _animator.pivotWeight;
        
        public AnimatorCullingMode cullingMode_ => _animator.cullingMode;

        public PlayableGraph playableGraph => _animator.playableGraph;
        
        public AnimatorRecorderMode recorderMode => _animator.recorderMode;
        
        public Vector3 targetPosition => _animator.targetPosition;
        
        public Quaternion targetRotation => _animator.targetRotation;
        
        public bool hasBoundPlayables => _animator.hasBoundPlayables;
        
        public bool hasRootMotion => _animator.hasRootMotion;
        
        public bool hasTransformHierarchy => _animator.hasTransformHierarchy;
        
        public bool isMatchingTarget => _animator.isMatchingTarget;

        public bool layersAffectMassCenter
        {
            get => _animator.layersAffectMassCenter;
            set
            {
                if (!IsController(_ownerAuth))
                    return;
                
                var setLayersAffectMassCenter = new SetLayersAffectMassCenter { value = value };
                setLayersAffectMassCenter.Apply(_animator);
                
                IfSameTypeReplace(new NetAnimatorRPC(setLayersAffectMassCenter));
            }
        }
        
        public bool logWarnings
        {
            get => _animator.logWarnings;
            set
            {
                if (!IsController(_ownerAuth))
                    return;
                
                var setLogWarnings = new SetLogWarnings { value = value };
                setLogWarnings.Apply(_animator);
                
                IfSameTypeReplace(new NetAnimatorRPC(setLogWarnings));
            }
        }
        
        public Vector3 rootPosition
        {
            get => _animator.rootPosition;
            set
            {
                if (!IsController(_ownerAuth))
                    return;
                
                var setRootPosition = new SetRootPosition { value = value };
                setRootPosition.Apply(_animator);
                
                IfSameTypeReplace(new NetAnimatorRPC(setRootPosition));
            }
        }
        
        public Quaternion rootRotation
        {
            get => _animator.rootRotation;
            set
            {
                if (!IsController(_ownerAuth))
                    return;
                
                var setRootRotation = new SetRootRotation { value = value };
                setRootRotation.Apply(_animator);
                
                IfSameTypeReplace(new NetAnimatorRPC(setRootRotation));
            }
        }
        
        public bool applyRootMotion
        {
            get => _animator.applyRootMotion;
            set
            {
                if (!IsController(_ownerAuth))
                    return;
                
                var setApplyRootMotion = new SetApplyRootMotion { value = value };
                setApplyRootMotion.Apply(_animator);
                
                IfSameTypeReplace(new NetAnimatorRPC(setApplyRootMotion));
            }
        }
        
        public bool writeDefaultValuesOnDisable
        {
            get => _animator.writeDefaultValuesOnDisable;
            set
            {
                if (!IsController(_ownerAuth))
                    return;
                
                var setWriteDefaultValuesOnDisable = new SetWriteDefaultValuesOnDisable { value = value };
                setWriteDefaultValuesOnDisable.Apply(_animator);
                
                IfSameTypeReplace(new NetAnimatorRPC(setWriteDefaultValuesOnDisable));
            }
        }
        
        public bool keepAnimatorStateOnDisable
        {
            get => _animator.keepAnimatorStateOnDisable;
            set
            {
                if (!IsController(_ownerAuth))
                    return;
                
                var setKeepAnimatorStateOnDisable = new SetKeepAnimatorStateOnDisable { value = value };
                setKeepAnimatorStateOnDisable.Apply(_animator);
                
                IfSameTypeReplace(new NetAnimatorRPC(setKeepAnimatorStateOnDisable));
            }
        }
        
        public float feetPivotActive
        {
            get => _animator.feetPivotActive;
            set
            {
                if (!IsController(_ownerAuth))
                    return;
                
                var setFeetPivotActive = new SetFeetPivotActive { value = value };
                setFeetPivotActive.Apply(_animator);
                
                IfSameTypeReplace(new NetAnimatorRPC(setFeetPivotActive));
            }
        }
        
        public bool stabilizeFeet
        {
            get => _animator.stabilizeFeet;
            set
            {
                if (!IsController(_ownerAuth))
                    return;
                
                var setStabilizeFeet = new SetStabilizeFeet { value = value };
                setStabilizeFeet.Apply(_animator);
                
                IfSameTypeReplace(new NetAnimatorRPC(setStabilizeFeet));
            }
        }
        
        public AnimatorUpdateMode updateMode
        {
            get => _animator.updateMode;
            set
            {
                if (!IsController(_ownerAuth))
                    return;
                
                var setUpdateMode = new SetUpdateMode { value = value };
                setUpdateMode.Apply(_animator);
                
                IfSameTypeReplace(new NetAnimatorRPC(setUpdateMode));
            }
        }
        
        public float playbackTime
        {
            get => _animator.playbackTime;
            set
            {
                if (!IsController(_ownerAuth))
                    return;
                
                var setPlaybackTime = new SetPlaybackTime { value = value };
                setPlaybackTime.Apply(_animator);
                
                IfSameTypeReplace(new NetAnimatorRPC(setPlaybackTime));
            }
        }
        
        public Vector3 bodyPosition
        {
            get => _animator.bodyPosition;
            set
            {
                if (!IsController(_ownerAuth))
                    return;
                
                var setBodyPosition = new SetBodyPosition { value = value };
                setBodyPosition.Apply(_animator);
                
                IfSameTypeReplace(new NetAnimatorRPC(setBodyPosition));
            }
        }
        
        public Quaternion bodyRotation
        {
            get => _animator.bodyRotation;
            set
            {
                if (!IsController(_ownerAuth))
                    return;
                
                var setBodyRotation = new SetBodyRotation { value = value };
                setBodyRotation.Apply(_animator);
                
                IfSameTypeReplace(new NetAnimatorRPC(setBodyRotation));
            }
        }
        
        public AnimatorCullingMode cullingMode
        {
            get => _animator.cullingMode;
            set
            {
                if (!IsController(_ownerAuth))
                    return;
                
                var setCullingMode = new SetCullingMode { value = value };
                setCullingMode.Apply(_animator);
                
                IfSameTypeReplace(new NetAnimatorRPC(setCullingMode));
            }
        }
        
        public bool fireEvents
        {
            get => _animator.fireEvents;
            set
            {
                if (!IsController(_ownerAuth))
                    return;
                
                var setFireEvents = new SetFireEvents { value = value };
                setFireEvents.Apply(_animator);
                
                IfSameTypeReplace(new NetAnimatorRPC(setFireEvents));
            }
        }
        
        public bool animatePhysics
        {
            get => _animator.animatePhysics;
            set
            {
                if (!IsController(_ownerAuth))
                    return;
                
                var setAnimatePhysics = new SetAnimatePhysics { value = value };
                setAnimatePhysics.Apply(_animator);
                
                IfSameTypeReplace(new NetAnimatorRPC(setAnimatePhysics));
            }
        }
        
        public float speed
        {
            get => _animator.speed;
            set
            {
                if (!IsController(_ownerAuth))
                    return;
                
                var setSpeed = new SetSpeed { speed = value };
                setSpeed.Apply(_animator);
                
                IfSameTypeReplace(new NetAnimatorRPC(setSpeed));
            }
        }
        
        public bool IsInTransition(int layerIndex) => _animator.IsInTransition(layerIndex);
        
        public T GetBehaviour<T>() where T : StateMachineBehaviour => _animator.GetBehaviour<T>();
        
        public StateMachineBehaviour[] GetBehaviours() => _animator.GetBehaviours<StateMachineBehaviour>();

        public T[] GetBehaviours<T>() where T : StateMachineBehaviour => _animator.GetBehaviours<T>();
        
        public AnimatorControllerParameter GetParameter(int index) => _animator.GetParameter(index);
        
        public AnimatorTransitionInfo GetAnimatorTransitionInfo(int layerIndex) => _animator.GetAnimatorTransitionInfo(layerIndex);
        
        public Vector3 GetIKPosition(AvatarIKGoal goal) => _animator.GetIKPosition(goal);
        
        public Quaternion GetIKRotation(AvatarIKGoal goal) => _animator.GetIKRotation(goal);
        
        public AnimatorStateInfo GetCurrentAnimatorStateInfo(int layerIndex) => _animator.GetCurrentAnimatorStateInfo(layerIndex);
        
        public AnimatorStateInfo GetNextAnimatorStateInfo(int layerIndex) => _animator.GetNextAnimatorStateInfo(layerIndex);
        
        public void GetCurrentAnimatorClipInfo(int layerIndex, List<AnimatorClipInfo> clips) => _animator.GetCurrentAnimatorClipInfo(layerIndex, clips);
        
        public void GetNextAnimatorClipInfo(int layerIndex, List<AnimatorClipInfo> clips) => _animator.GetNextAnimatorClipInfo(layerIndex, clips);
        
        public Vector3 GetIKHintPosition(AvatarIKHint hint) => _animator.GetIKHintPosition(hint);
        
        public float GetIKPositionWeight(AvatarIKGoal goal) => _animator.GetIKPositionWeight(goal);
        
        public float GetIKRotationWeight(AvatarIKGoal goal) => _animator.GetIKRotationWeight(goal);
        
        public bool IsParameterControlledByCurve(string paramName) => _animator.IsParameterControlledByCurve(paramName);
        
        public int GetCurrentAnimatorClipInfoCount(int layerIndex) => _animator.GetCurrentAnimatorClipInfoCount(layerIndex);
        
        public float GetIKHintPositionWeight(AvatarIKHint hint) => _animator.GetIKHintPositionWeight(hint);
        
        public int GetNextAnimatorClipInfoCount(int layerIndex) => _animator.GetNextAnimatorClipInfoCount(layerIndex);
        
        public int GetLayerIndex(string layerName) => _animator.GetLayerIndex(layerName);
        
        public string GetLayerName(int layerIndex) => _animator.GetLayerName(layerIndex);
        
        public float GetLayerWeight(int layerIndex) => _animator.GetLayerWeight(layerIndex);
        
        public Transform GetBoneTransform(HumanBodyBones humanBoneId) => _animator.GetBoneTransform(humanBoneId);
        
        public bool HasState(int layerIndex, int stateID) => _animator.HasState(layerIndex, stateID);

        public void SetTrigger(string triggerName) => SetTrigger(Animator.StringToHash(triggerName));
        
        public void ResetTrigger(string triggerName) => ResetTrigger(Animator.StringToHash(triggerName));
        
        public void SetFloat(string propName, float value) => SetFloat(Animator.StringToHash(propName), value);
        
        public float GetFloat(string propName) => _animator.GetFloat(propName);
        
        public void SetBool(string propName, bool value) => SetBool(Animator.StringToHash(propName), value);
        
        public bool GetBool(string propName) => _animator.GetBool(propName);
        
        public bool GetBool(int nameHash) => _animator.GetBool(nameHash);

        public void SetInt(string propName, int value) => SetInt(Animator.StringToHash(propName), value);
        
        public int GetInteger(string propName) => _animator.GetInteger(propName);
        
        public int GetInteger(int nameHash) => _animator.GetInteger(nameHash);

        public void ResetTrigger(int nameHash)
        {
            if (!IsController(_ownerAuth))
                return;
            
            var resetTrigger = new ResetTrigger {nameHash = nameHash};
            resetTrigger.Apply(_animator);
            
            IfSameReplace(new NetAnimatorRPC(resetTrigger), 
                (a, b) => a._trigger.nameHash == b._trigger.nameHash);
        }

        public void SetTrigger(int nameHash)
        {
            if (!IsController(_ownerAuth))
                return;
            
            var trigger = new SetTrigger {nameHash = nameHash};
            trigger.Apply(_animator);
            
            IfSameReplace(new NetAnimatorRPC(trigger), 
                (a, b) => a._trigger.nameHash == b._trigger.nameHash);
        }

        public void SetFloat(int nameHash, float value)
        {
            if (!IsController(_ownerAuth))
                return;
            
            var setFloat = new SetFloat {nameHash = nameHash, value = value};
            setFloat.Apply(_animator);
            _floatValues[nameHash] = value;

            IfSameReplace(new NetAnimatorRPC(setFloat), 
                (a, b) => a._float.nameHash == b._float.nameHash);
        }
        
        public void SetBool(int nameHash, bool value)
        {
            if (!IsController(_ownerAuth))
                return;
            
            var setBool = new SetBool {nameHash = nameHash, value = value};
            setBool.Apply(_animator);
            _boolValues[nameHash] = value;
            
            IfSameReplace(new NetAnimatorRPC(setBool), 
                (a, b) => a._bool.nameHash == b._bool.nameHash);
        }
        
        public void SetInt(int nameHash, int value)
        {
            if (!IsController(_ownerAuth))
                return;
            
            var setInt = new SetInt {nameHash = nameHash, value = value};
            setInt.Apply(_animator);
            _intValues[nameHash] = value;
            
            IfSameReplace(new NetAnimatorRPC(setInt), 
                (a, b) => a._int.nameHash == b._int.nameHash);
        }
        
        public void Play(string stateName, int layer)
        {
            var hash = Animator.StringToHash(stateName);
            Play(hash, layer);
        }

        public void Play(string stateName)
        {
            var hash = Animator.StringToHash(stateName);
            Play(hash);
        }

        public void Play(string stateName, [UnityEngine.Internal.DefaultValue("-1")] int layer, [UnityEngine.Internal.DefaultValue("float.NegativeInfinity")] float normalizedTime)
        {
            var hash = Animator.StringToHash(stateName);
            Play(hash, layer, normalizedTime);
        }

        public void Play(int stateNameHash, [UnityEngine.Internal.DefaultValue("-1")] int layer, [UnityEngine.Internal.DefaultValue("float.NegativeInfinity")] float normalizedTime)
        {
            if (!IsController(_ownerAuth))
                return;
            
            var play = new Play_STATEHASH_LAYER_NORMALIZEDTIME {stateHash = stateNameHash, layer = layer, normalizedTime = normalizedTime};
            play.Apply(_animator);
            
            _dirty.Add(new NetAnimatorRPC(play));
        }

        public void Play(int stateNameHash, int layer)
        {
            if (!IsController(_ownerAuth))
                return;
            
            var play = new Play_STATEHASH_LAYER {stateHash = stateNameHash, layer = layer};
            play.Apply(_animator);
            
            _dirty.Add(new NetAnimatorRPC(play));
        }

        public void Play(int stateNameHash)
        {
            if (!IsController(_ownerAuth))
                return;
            
            var play = new PLAY_STATEHASH {stateHash = stateNameHash};
            play.Apply(_animator);
            
            _dirty.Add(new NetAnimatorRPC(play));
        }

        public void Rebind()
        {
            if (!IsController(_ownerAuth))
                return;
            
            var rebind = new Rebind();
            rebind.Apply(_animator);
            
            IfSameTypeReplace(new NetAnimatorRPC(rebind));
        }

        /// <summary>
        /// Same as Animator.Update(float deltaTime)
        /// Due to current limitations we had to rename this method
        /// </summary>
        /// <param name="deltaTime"></param>
        public void UpdateWithDelta(float deltaTime)
        {
            if (!IsController(_ownerAuth))
                return;
            
            var update = new UpdateWithDelta { delta = deltaTime};
            update.Apply(_animator);
            
            _dirty.Add(new NetAnimatorRPC(update));
        }

        public void CrossFade(
            string stateName,
            float normalizedTransitionDuration,
            int layer,
            float normalizedTimeOffset)
        {
            float normalizedTransitionTime = 0.0f;
            CrossFade(stateName, normalizedTransitionDuration, layer, normalizedTimeOffset, normalizedTransitionTime);
        }

        public void CrossFade(string stateName, float normalizedTransitionDuration, int layer)
        {
            CrossFade(stateName, normalizedTransitionDuration, layer, float.NegativeInfinity, 0f);
        }

        public void CrossFade(string stateName, float normalizedTransitionDuration)
        {
            CrossFade(stateName, normalizedTransitionDuration, -1, float.NegativeInfinity, 0f);
        }

        public void CrossFade(
            string stateName,
            float normalizedTransitionDuration,
            [UnityEngine.Internal.DefaultValue("-1")]
            int layer,
            [UnityEngine.Internal.DefaultValue("float.NegativeInfinity")]
            float normalizedTimeOffset,
            [UnityEngine.Internal.DefaultValue("0.0f")]
            float normalizedTransitionTime)
        {
            CrossFade(Animator.StringToHash(stateName), normalizedTransitionDuration, layer, normalizedTimeOffset,
                normalizedTransitionTime);
        }

        public void CrossFade(
            int stateHashName,
            float normalizedTransitionDuration,
            [UnityEngine.Internal.DefaultValue("-1")]
            int layer,
            [UnityEngine.Internal.DefaultValue("0.0f")]
            float normalizedTimeOffset,
            [UnityEngine.Internal.DefaultValue("0.0f")]
            float normalizedTransitionTime)
        {
            if (!IsController(_ownerAuth))
                return;
            
            var crossFade = new CrossFade_5
            {
                stateHash = stateHashName, 
                normalizedTime = normalizedTransitionDuration,
                layer = layer,
                fixedTime = normalizedTimeOffset, 
                fixedDuration = normalizedTransitionTime
            };
            
            crossFade.Apply(_animator);
            
            _dirty.Add(new NetAnimatorRPC(crossFade));
        }

        public void CrossFade(
            int stateHashName,
            float normalizedTransitionDuration,
            int layer,
            float normalizedTimeOffset)
        {
            if (!IsController(_ownerAuth))
                return;
            
            var crossFade = new CrossFade_4
            {
                stateHash = stateHashName,
                normalizedTime = normalizedTransitionDuration,
                layer = layer,
                fixedDuration = normalizedTimeOffset
            };
            
            crossFade.Apply(_animator);
            
            _dirty.Add(new NetAnimatorRPC(crossFade));
        }

        public void CrossFade(int stateHashName, float normalizedTransitionDuration, int layer)
        {
            if (!IsController(_ownerAuth))
                return;
            
            var crossFade = new CrossFade_3
            {
                stateHash = stateHashName,
                normalizedTime = normalizedTransitionDuration,
                layer = layer
            };
            
            crossFade.Apply(_animator);
            
            _dirty.Add(new NetAnimatorRPC(crossFade));
        }

        public void CrossFade(int stateHashName, float normalizedTransitionDuration)
        {
            if (!IsController(_ownerAuth))
                return;
            
            var crossFade = new CrossFade_2
            {
                stateHash = stateHashName,
                normalizedTime = normalizedTransitionDuration
            };
            
            crossFade.Apply(_animator);
            
            _dirty.Add(new NetAnimatorRPC(crossFade));
        }
        
        public void MatchTarget(
            Vector3 matchPosition,
            Quaternion matchRotation,
            AvatarTarget targetBodyPart,
            MatchTargetWeightMask weightMask,
            float startNormalizedTime,
            float targetNormalizedTime = 1f,
            bool completeMatch = true)
        {
            if (!IsController(_ownerAuth))
                return;
            
            var matchTarget = new MatchTarget
            {
                matchPosition = matchPosition,
                matchRotation = matchRotation,
                targetBodyPart = targetBodyPart,
                weightMask = weightMask,
                startNormalizedTime = startNormalizedTime,
                targetNormalizedTime = targetNormalizedTime,
                completeMatch = completeMatch
            };
            
            matchTarget.Apply(_animator);
            
            _dirty.Add(new NetAnimatorRPC(matchTarget));
        }
        
        public void InterruptMatchTarget(bool completeMatch = true)
        {
            if (!IsController(_ownerAuth))
                return;
            
            var interruptMatchTarget = new InterruptMatchTarget {completeMatch = completeMatch};
            interruptMatchTarget.Apply(_animator);
            
            IfSameTypeReplace(new NetAnimatorRPC(interruptMatchTarget));
        }
        
        public void SetLayerWeight(int layerIndex, float weight)
        {
            if (!IsController(_ownerAuth))
                return;
            
            var setLayerWeight = new SetLayerWeight {layerIndex = layerIndex, weight = weight};
            setLayerWeight.Apply(_animator);
            
            IfSameReplace(new NetAnimatorRPC(setLayerWeight), 
                (a, b) => a._setLayerWeight.layerIndex == b._setLayerWeight.layerIndex);
        }
        
        public void WriteDefaultValues()
        {
            if (!IsController(_ownerAuth))
                return;
            
            var writeDefaultValues = new WriteDefaultValues();
            writeDefaultValues.Apply(_animator);
            
            IfSameTypeReplace(new NetAnimatorRPC(writeDefaultValues));
        }
        
        
        public void ApplyBuiltinRootMotion()
        {
            if (!IsController(_ownerAuth))
                return;
            
            var applyBuiltinRootMotion = new ApplyBuiltinRootMotion();
            applyBuiltinRootMotion.Apply(_animator);
            
            IfSameTypeReplace(new NetAnimatorRPC(applyBuiltinRootMotion));
        }
        
        public void PlayInFixedTime(string stateName, int layer = -1, float fixedTime = float.NegativeInfinity)
        {
            PlayInFixedTime(Animator.StringToHash(stateName), layer, fixedTime);
        }
        
        public void PlayInFixedTime(int stateNameHash, int layer = -1, float fixedTime = float.NegativeInfinity)
        {
            if (!IsController(_ownerAuth))
                return;
            
            var playInFixedTime = new PlayInFixedTime {stateHash = stateNameHash, layer = layer, fixedTime = fixedTime};
            playInFixedTime.Apply(_animator);
            
            _dirty.Add(new NetAnimatorRPC(playInFixedTime));
        }

        public void SetBoneLocalRotation(HumanBodyBones humanBoneId, Quaternion rotation)
        {
            if (!IsController(_ownerAuth))
                return;
            
            var setBoneLocalRotation = new SetBoneLocalRotation {bone = humanBoneId, rotation = rotation};
            setBoneLocalRotation.Apply(_animator);
            
            IfSameReplace(new NetAnimatorRPC(setBoneLocalRotation), 
                (a, b) => a._setBoneLocalRotation.bone == b._setBoneLocalRotation.bone);
        }
        
        public void SetIKPosition(AvatarIKGoal goal, Vector3 position)
        {
            if (!IsController(_ownerAuth))
                return;
            
            var setIKPosition = new SetIKPosition {goal = goal, position = position};
            setIKPosition.Apply(_animator);
            
            IfSameReplace(new NetAnimatorRPC(setIKPosition), 
                (a, b) => a._setIKPosition.goal == b._setIKPosition.goal);
        }
        
        public void SetIKRotation(AvatarIKGoal goal, Quaternion rotation)
        {
            if (!IsController(_ownerAuth))
                return;
            
            var setIKRotation = new SetIKRotation {goal = goal, rotation = rotation};
            setIKRotation.Apply(_animator);
            
            IfSameReplace(new NetAnimatorRPC(setIKRotation), 
                (a, b) => a._setIKRotation.goal == b._setIKRotation.goal);
        }
        
        public void SetLookAtPosition(Vector3 lookAtPosition)
        {
            if (!IsController(_ownerAuth))
                return;
            
            var setLookAtPosition = new SetLookAtPosition {position = lookAtPosition};
            setLookAtPosition.Apply(_animator);
            
            IfSameTypeReplace(new NetAnimatorRPC(setLookAtPosition));
        }

        public void SetLookAtWeight(
            float weight,
            float bodyWeight = 0.0f,
            float headWeight = 1.0f,
            float eyesWeight = 0.0f,
            float clampWeight = 0.5f)
        {
            if (!IsController(_ownerAuth))
                return;
            
            var setLookAtWeight = new SetLookAtWeight
            {
                weight = weight,
                bodyWeight = bodyWeight,
                headWeight = headWeight,
                eyesWeight = eyesWeight,
                clampWeight = clampWeight
            };
            
            setLookAtWeight.Apply(_animator);
            
            IfSameTypeReplace(new NetAnimatorRPC(setLookAtWeight));
        }
        
        public void CrossFadeInFixedTime(
            string stateHashName,
            float fixedTransitionDuration,
            int layer = -1,
            float fixedTimeOffset = 0f,
            float normalizedTransitionTime = 0f)
        {
            CrossFadeInFixedTime(Animator.StringToHash(stateHashName), fixedTransitionDuration, layer, fixedTimeOffset, normalizedTransitionTime);
        }

        public void CrossFadeInFixedTime(
            int stateHashName,
            float fixedTransitionDuration,
            int layer = -1,
            float fixedTimeOffset = 0f,
            float normalizedTransitionTime = 0f)
        {
            if (!IsController(_ownerAuth))
                return;
            
            var crossFadeInFixedTime = new CrossFadeInFixedTime
            {
                stateHash = stateHashName,
                fixedTime = fixedTransitionDuration,
                layer = layer,
                fixedDuration = fixedTimeOffset,
                normalizedTime = normalizedTransitionTime
            };
            
            crossFadeInFixedTime.Apply(_animator);
            
            _dirty.Add(new NetAnimatorRPC(crossFadeInFixedTime));
        }
        
        public void SetIKHintPosition(AvatarIKHint hint, Vector3 position)
        {
            if (!IsController(_ownerAuth))
                return;
            
            var setIKHintPosition = new SetIKHintPosition {hint = hint, position = position};
            setIKHintPosition.Apply(_animator);
            
            IfSameReplace(new NetAnimatorRPC(setIKHintPosition), 
                (a, b) => a._setIKHintPosition.hint == b._setIKHintPosition.hint);
        }
        
        public void SetIKPositionWeight(AvatarIKGoal goal, float value)
        {
            if (!IsController(_ownerAuth))
                return;
            
            var setIKPositionWeight = new SetIKPositionWeight {goal = goal, value = value};
            setIKPositionWeight.Apply(_animator);
            
            IfSameReplace(new NetAnimatorRPC(setIKPositionWeight), 
                (a, b) => a._setIKPositionWeight.goal == b._setIKPositionWeight.goal);
        }
        
        public void SetIKRotationWeight(AvatarIKGoal goal, float value)
        {
            if (!IsController(_ownerAuth))
                return;
            
            var setIKRotationWeight = new SetIKRotationWeight {goal = goal, value = value};
            setIKRotationWeight.Apply(_animator);
            
            IfSameReplace(new NetAnimatorRPC(setIKRotationWeight), 
                (a, b) => a._setIKRotationWeight.goal == b._setIKRotationWeight.goal);
        }
        
        public void SetIKHintPositionWeight(AvatarIKHint hint, float value)
        {
            if (!IsController(_ownerAuth))
                return;
            
            var setIKHintPositionWeight = new SetIKHintPositionWeight {hint = hint, value = value};
            setIKHintPositionWeight.Apply(_animator);
            
            IfSameReplace(new NetAnimatorRPC(setIKHintPositionWeight), 
                (a, b) => a._setIKHintPositionWeight.hint == b._setIKHintPositionWeight.hint);
        }
    }
}
