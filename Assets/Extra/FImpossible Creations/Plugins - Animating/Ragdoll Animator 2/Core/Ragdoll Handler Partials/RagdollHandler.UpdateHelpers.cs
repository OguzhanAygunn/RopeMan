namespace FIMSpace.FProceduralAnimation
{
    public partial class RagdollHandler
    {
        public void PreCalibrate()
        {
            //if( ApplyPositions )
            //{
            //    foreach( var chain in chains ) chain.Calibrate();
            //}
            //else
            //{
            //    _playmodeAnchorBone.BoneProcessor.Calibrate();
            //    foreach( var chain in chains ) chain.CalibrateJustRotation();
            //}

            if( ApplyPositions )
            {
                if( IsInStandingMode )
                {
                    foreach( var chain in chains ) chain.Calibrate();
                }
                else // Calibrate all, except anchor bone
                {
                    foreach( var chain in chains ) foreach( var proc in chain.RuntimeBoneProcessors ) { proc.Calibrate(); }
                }
            }
            else
            {
                if( IsInStandingMode )
                {
                    _playmodeAnchorBone.BoneProcessor.Calibrate(); // Calibrate anchror position all the time
                    foreach( var chain in chains ) chain.CalibrateJustRotation();
                }
                else // Calibrate all, except anchor bone
                {
                    foreach( var chain in chains ) foreach( var proc in chain.RuntimeBoneProcessors ) { proc.CalibrateRotation(); }
                }
            }
        }

        /// <summary>
        /// (Runtime) Updating joint dynamic parameters like connected mass scale etc.
        /// </summary>
        public void RefreshAllChainsDynamicParameters()
        {
            bool fall = IsFallingOrSleep;

            foreach( var chain in chains )
            {
                foreach( var bone in chain.BoneSetups )
                {
                    bone.RefreshDynamicPhysicalParameters( chain, fall );
                    bone.RefreshJointLimitSwitch( chain );
                }
            }
        }

        /// <summary>
        /// (Runtime) Updating joint dynamic parameters like connected mass scale etc.
        /// </summary>
        public void RefreshAllChainsRigidbodyOptimizationParameters()
        {
            foreach( var chain in chains )
            {
                foreach( var bone in chain.BoneSetups )
                {
                    bone.RefreshRigidbodyOptimizationParameters( this );
                }
            }
        }
    }
}