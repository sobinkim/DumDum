using Core.EventBus;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


public abstract class Entity : ExtendedMono
{ public bool IsInvin { get; set; } = false;
    protected Dictionary<Type, IEntityComponent> _components;

    public void EntityDestroy()
    {
        Destroy(gameObject);
    }

    protected virtual void Awake()
    {
        _components = new Dictionary<Type, IEntityComponent>();

        AddComponents();
        InitializeComponents();
        AfterInitializeComponents();
    }
    
    protected virtual void AddComponents()
    {
        GetComponentsInChildren<IEntityComponent>().ToList()
            .ForEach(component => _components.Add(component.GetType(), component));
    }

    protected virtual void InitializeComponents()
    {
        _components.Values.ToList().ForEach(component => component.Initialize(this));
    }

    protected void AfterInitializeComponents()
    {
        _components.Values.OfType<IAfterInitialize>()
            .ToList().ForEach(component => component.AfterInitialize());
    }

    public T GetCompo<T>() where T : IEntityComponent
    {
        foreach (var comp in _components.Values)
        {
            if (comp is T t)
                return t;
        }
        return default;
    }

    public IEntityComponent GetCompo(Type type)
        => _components.GetValueOrDefault(type);

   
    public void SetInvincible(bool invin) => IsInvin = invin;
}
