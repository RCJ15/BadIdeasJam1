using System;

/// <summary>
/// An enum used to determine how the <see cref="CacheComponentAttribute"/> will cache it's <see cref="UnityEngine.Component"/>.
/// </summary>
[Flags]
public enum CacheMethod
{
    /// <summary>
    /// Does GetComponent.
    /// </summary>
    Default = 1,
    /// <summary>
    /// Does GetComponentInChildren.
    /// </summary>
    InChildren = 2,
    /// <summary>
    /// Does GetComponentInParent.
    /// </summary>
    InParent = 4,
    /// <summary>
    /// Sets the "includeInactive" bool to true when using any GetComponent.
    /// </summary>
    IncludeInactive = 8,
    /// <summary>
    /// Does GetComponentInChildren with the "includeInactive" bool set to true.
    /// </summary>
    InChildrenInactive = InChildren | IncludeInactive,
    /// <summary>
    /// Does GetComponentInParent with the "includeInactive" bool set to true.
    /// </summary>
    InParentInactive = InParent | IncludeInactive,
    /// <summary>
    /// Does GetComponentInChildren. If that fails it does GetComponentInParent.
    /// </summary>
    All = Default | InChildren | InParent,
    /// <summary>
    /// Does GetComponentInChildren. If that fails it does GetComponentInParent. All with the "includeInactive" bool set to true.
    /// </summary>
    AllAndInactive = Default | InChildren | InParent | IncludeInactive,
}