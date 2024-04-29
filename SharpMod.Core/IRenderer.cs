using System;

namespace SharpMod
{
    ///<summary>
    ///</summary>
    public interface IRenderer : IDisposable
    {
        ///<summary>
        ///</summary>
        void Init();
        ///<summary>
        ///</summary>
        void PlayStart();
        ///<summary>
        ///</summary>
        void PlayStop();
        ///<summary>
        ///</summary>
        ModulePlayer Player { get; set; }

    }
}
