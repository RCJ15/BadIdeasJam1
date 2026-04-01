using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// A script that will make sure only 1 instance of itself exists per scene. <para/>
/// Has options to allow for persistance between scenes using the <see cref="SingletonModeAttribute"/>. <para/>
/// IMPORTANT NOTE: When extending you must make sure that the <see cref="T"/> is exactly the type of the script that you're creating.
/// </summary>
[SingletonMode]
public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    public const string FOLDER = "Singletons";

    /// <summary>
    /// The singular instance of <typeparamref name="T"/>.
    /// </summary>
    public static T Instance { get; private set; }
    private static readonly Type ThisType = typeof(T);

    /*
    /// <summary>
    /// Is true if this instance was destroyed in the <see cref="Awake"/> method. <para/>
    /// Use this to return out of doing any code in the <see cref="Awake"/> method in case this instance got destroyed.
    /// </summary>
    protected bool isDestroyed;
    */

    internal GameObject rootObject;

    protected virtual void Awake()
    {
        SingletonModeAttribute attribute;

        // Get attribute
        if (SingletonModeAttribute.Attributes.ContainsKey(ThisType))
        {
            attribute = SingletonModeAttribute.Attributes[ThisType];
        }
        // Fallback attribute
        else
        {
            attribute = SingletonModeAttribute.Attributes[typeof(Singleton<>)];
        }

        // Destroy this singleton instance if an instance already exists
        /*
        if (Instance != null)
        {
            isDestroyed = true;
            Destroy(transform.root.gameObject);
            return; // Make sure to return so nothing below will happen
        }
        */

        // Set instance
        Instance = (T)this;

        // Set this object to not destroy on load if the object has CreateOnInitialize set to true
        if (attribute.CreateOnInitialize)
        {
            DontDestroyOnLoad(transform.root.gameObject);
        }
    }
}

/// <summary>
/// The <see cref="Attribute"/> applied to all <see cref="Singleton{T}"/> classes. <para/>
/// This attribute controls how a <see cref="Singleton{T}"/> script will act using a set of bools.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class SingletonModeAttribute : Attribute
{
    // Dictionary that caches all attributes on every single singleton script
    public static Dictionary<Type, SingletonModeAttribute> Attributes = new Dictionary<Type, SingletonModeAttribute>();

    /// <summary>
    /// A bool that determines if this <see cref="Singleton{T}"/> should be created in when the game is initialized.
    /// </summary>
    public bool CreateOnInitialize => _createOnInitialize;
    private bool _createOnInitialize = false;

    public SingletonModeAttribute()
    {

    }

    public SingletonModeAttribute(bool createOnInitialize) : this()
    {
        _createOnInitialize = createOnInitialize;
    }

    /// <summary>
    /// Loads all attributes into the <see cref="Attributes"/> array.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    private static void InitAttributes()
    {
        //-- Find all classes that implement this attribute and add them to the dictionary
        Type attributeType = typeof(SingletonModeAttribute);

        // Loop through all assemblies in the app domain
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            // Loop through all types in the assembly
            foreach (Type type in assembly.GetTypes())
            {
                // Get the attribute from the type
                SingletonModeAttribute attribute = type.GetCustomAttribute(attributeType) as SingletonModeAttribute;

                // Check if the attribute exists
                if (attribute != null)
                {
                    // Add to dictionary
                    Attributes.Add(type, attribute);
                }
            }
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitCreateOnInitializeMethods()
    {
        foreach (var pair in Attributes)
        {
            if (!pair.Value.CreateOnInitialize)
            {
                continue;
            }

            _loadSingletonMethod.MakeGenericMethod(pair.Key).Invoke(null, null);
        }
    }

    private static readonly MethodInfo _loadSingletonMethod = typeof(SingletonModeAttribute).GetMethod(nameof(LoadSingleton), BindingFlags.Static | BindingFlags.NonPublic);
    private static void LoadSingleton<T>() where T : Singleton<T>
    {
        string name = typeof(T).Name;

        GameObject newObj;
        T component;

        // Create instance
        newObj = Resources.Load<GameObject>(Singleton<T>.FOLDER + "/" + name);

        if (newObj == null)
        {
            Debug.LogError("Could not find singleton prefab for " + name + " in folder: " + Singleton<T>.FOLDER);
            return;
        }

        newObj = UnityEngine.Object.Instantiate(newObj);
        newObj.name = name;
        component = newObj.GetComponentInChildren<T>(true);

        // Set root object
        if (newObj != component.gameObject)
        {
            component.rootObject = newObj;
        }
    }
}
