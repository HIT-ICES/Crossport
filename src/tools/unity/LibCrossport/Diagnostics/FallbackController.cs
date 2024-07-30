using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
namespace Anonymous.Crossport.Diagnostics
{
public class FallbackController : MonoBehaviour
{
    /// <summary>
    /// Use remote renderer
    /// </summary>
    public static void UseRemote()
    {
        _instance?.ManagedElements.ForEach(e=>e.SetActive(false));
    }
    /// <summary>
    /// Use local renderer
    /// </summary>
    public static void UseLocal()
    {
        _instance?.ManagedElements.ForEach(e=>e.SetActive(true));
    }
    [CanBeNull] protected static FallbackController _instance;
    /// <summary>
    /// Elements that requires remote rendering by default
    /// </summary>
    [SerializeField] protected List<GameObject> ManagedElements = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        _instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
}