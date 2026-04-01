using System;
using UnityEngine;

/// <summary>
/// Use this attribute on a <see cref="Component"/> field that also has a <see cref="SerializeField"/> attribute to make it automatically use GetComponent and cache the value in the inspector.<para/>
/// NOTE: Doesn't work with arrays of Components, but it works together with a <see cref="ComponentCollection{T}"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class CacheComponentAttribute : PropertyAttribute
{
    /// <summary>
    /// How this attribute will cache the <see cref="Component"/>.
    /// </summary>
    public CacheMethod CacheMethod => _cacheMethod;
    private CacheMethod _cacheMethod = CacheMethod.InChildrenInactive;

    /// <summary>
    /// Whether this field should be drawn in the inspector. This is true by default.<para/>
    /// Works exactly like how <see cref="HideInInspector"/> works except that the component will actually be cached. (It won't be cached if you use <see cref="HideInInspector"/>)
    /// </summary>
    public bool DrawField = true;
    /// <summary>
    /// Whether this field should be drawn as a disabled field in the inspector. This is true by default.
    /// </summary>
    public bool DisableField = true;
    /// <summary>
    /// Whether this field should automatically cache every oppurtunity it has. This is false by default.
    /// </summary>
    public bool AlwaysCache = false;
    /// <summary>
    /// If the "Cache" button on a <see cref="ComponentCollection{T}"/> should be hidden. This is false by default.<para/>
    /// Only works when this attribute is on a <see cref="ComponentCollection{T}"/>.
    /// </summary>
    public bool HideCacheButton = false;

    /// <summary>
    /// Will return wether or not a <see cref="ComponentCollection{T}"/> should have a "Cache" button.
    /// </summary>
    public bool HaveCacheButton() => !AlwaysCache && !HideCacheButton;

    /// <summary>
    /// Creates a <see cref="CacheComponentAttribute"/> with the default CacheMethod.
    /// </summary>
    public CacheComponentAttribute()
    {

    }

    /// <summary>
    /// Creates a <see cref="CacheComponentAttribute"/> with the given CacheMethod.
    /// </summary>
    public CacheComponentAttribute(CacheMethod cacheMethod)
    {
        _cacheMethod = cacheMethod;
    }
}
