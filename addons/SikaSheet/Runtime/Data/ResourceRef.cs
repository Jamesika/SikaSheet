using System;
using Godot;

namespace SikaSheet;

public interface IResourceRef
{
    public string Uid { get; set; }
    public string Path { get; set; }
    public Resource RawValue
    {
        get
        {
            var uidNumber = ResourceUid.TextToId(Uid);
            if (ResourceUid.HasId(uidNumber))
                return ResourceLoader.Load(Uid);
            
            if(!string.IsNullOrEmpty(Path))
                return ResourceLoader.Load(Path);

            return null;
        }
    }
    
    public Type RequiredResourceType => typeof(Resource);

    public bool Validate()
    {
        if (string.IsNullOrEmpty(Uid) && string.IsNullOrEmpty(Path))
            return true;
        
        var uidNumber = ResourceUid.TextToId(Uid);
        if (ResourceUid.HasId(uidNumber))
        {
            var res = ResourceLoader.Load(Uid);
            return IsAssignable(res);
        }

        return false;
    }

    /// <summary>
    /// Call Fix when load sheet & file system changed.
    /// </summary>
    public bool TryFixResource()
    {
        if (string.IsNullOrEmpty(Uid) && string.IsNullOrEmpty(Path))
            return false;
        
        var uidNumber = ResourceUid.TextToId(Uid);
        if (ResourceUid.HasId(uidNumber))
        {
            var idPath = ResourceUid.GetIdPath(uidNumber);
            if (idPath != Path)
            {
                Path = idPath;
                return true;
            }

            return false;
        }

        if (!string.IsNullOrEmpty(Path))
        {
            var uid = ResourceLoader.GetResourceUid(Path);
            if (ResourceUid.HasId(uid))
            {
                Uid = ResourceUid.IdToText(uid);
                return true;
            }
        }

        return false;
    }

    public bool IsAssignable(Resource resource)
    {
        return resource == null || resource.GetType().IsAssignableTo(RequiredResourceType);
    }

    public void UpdateResource(Resource resource)
    {
        if (!IsAssignable(resource))
        {
            SheetLogger.LogError("Cant assign resource");
            return;
        }

        if (resource == null)
        {
            Uid = string.Empty;
            Path = string.Empty;
            return;
        }

        Path = resource.ResourcePath;
        var uidNum = ResourceLoader.GetResourceUid(Path);
        Uid = ResourceUid.IdToText(uidNum);
    }
}

public struct ResourceRef<T> : IResourceRef where T : Resource
{
    public string Uid { get; set; }
    public string Path { get; set; }
    public T Value => (this as IResourceRef).RawValue as T;
    public Type RequiredResourceType => typeof(T);
}