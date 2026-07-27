namespace IERIC.SumariosIERIC.Domain.SeedWork
{
    using MediatR;
    using IERIC.SumariosIERIC.Services.CursosService.Domain.SeedWork;
    using System;
    using System.Collections.Generic;

    public abstract class Entity : ITrack
    {
        int? _requestedHashCode;
        Guid _Id;
        public virtual Guid Id
        {
            get
            {
                return _Id;
            }
            set
            {
                _Id = value;
            }
        }

        int _LegacyId;

        public virtual int LegacyId
        {
            get
            {
                return _LegacyId;
            }
            protected set
            {
                _LegacyId = value;
            }
        }

        bool _Activo;
        public virtual bool Activo
        {
            get
            {
                return _Activo;
            }
            set
            {
                _Activo = value;
            }
        }
        DateTime _FechaAlta;
        public virtual DateTime FechaAlta
        {
            get
            {
                return _FechaAlta;
            }
            set
            {
                _FechaAlta = value;
            }
        }

        string _UsuarioAlta;
        public virtual string UsuarioAlta
        {
            get
            {
                return _UsuarioAlta;
            }
            set
            {
                _UsuarioAlta = value;
            }
        }
        DateTime _FechaUpdate;
        public virtual DateTime FechaUpdate
        {
            get
            {
                return _FechaUpdate;
            }
            set
            {
                _FechaUpdate = value;
            }
        }

        string _UsuarioUpdate;
        public virtual string UsuarioUpdate
        {
            get
            {
                return _UsuarioUpdate;
            }
            set
            {
                _UsuarioUpdate = value;
            }
        }


        private List<INotification> _domainEvents;
        public IReadOnlyCollection<INotification> DomainEvents => _domainEvents?.AsReadOnly();

        public void AddDomainEvent(INotification eventItem)
        {
            _domainEvents = _domainEvents ?? new List<INotification>();
            _domainEvents.Add(eventItem);
        }

        public void RemoveDomainEvent(INotification eventItem)
        {
            _domainEvents?.Remove(eventItem);
        }

        public void ClearDomainEvents()
        {
            _domainEvents?.Clear();
        }

        public bool IsTransient()
        {
            return this.Id == default(Guid);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Entity))
                return false;

            if (Object.ReferenceEquals(this, obj))
                return true;

            if (this.GetType() != obj.GetType())
                return false;

            Entity item = (Entity)obj;

            if (item.IsTransient() || this.IsTransient())
                return false;
            else
                return item.Id == this.Id;
        }

        public override int GetHashCode()
        {
            if (!IsTransient())
            {
                if (!_requestedHashCode.HasValue)
                    _requestedHashCode = this.Id.GetHashCode() ^ 31; // XOR for random distribution (http://blogs.msdn.com/b/ericlippert/archive/2011/02/28/guidelines-and-rules-for-gethashcode.aspx)

                return _requestedHashCode.Value;
            }
            else
                return base.GetHashCode();

        }
        public static bool operator ==(Entity left, Entity right)
        {
            if (Object.Equals(left, null))
                return (Object.Equals(right, null)) ? true : false;
            else
                return left.Equals(right);
        }

        public static bool operator !=(Entity left, Entity right)
        {
            return !(left == right);
        }
    }
}

