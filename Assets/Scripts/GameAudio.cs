using UnityEngine;
using System.Collections.Generic;
public class GameAudio : MonoBehaviour
{
    static AudioSource source;
    static readonly Dictionary<string,AudioClip> clips=new Dictionary<string,AudioClip>();
    public static bool Muted;
    void Awake(){source=gameObject.AddComponent<AudioSource>();source.volume=.16f;source.playOnAwake=false;}
    public static void Play(string cue)
    {
        if(Muted||source==null)return;
        if(!clips.TryGetValue(cue,out var clip)||clip==null){clip=Resources.Load<AudioClip>("Audio/"+cue);clips[cue]=clip;}
        if(clip!=null)source.PlayOneShot(clip);
        else Debug.LogWarning("Asset de áudio ausente: "+cue);
    }
}
