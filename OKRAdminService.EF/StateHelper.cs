using Microsoft.EntityFrameworkCore;
using System;

namespace OKRAdminService.EF
{
    public static class StateHelper
    {
        public static EntityState ConvertState( ObjectState state )
        {
            switch ( state )
            {
                case ObjectState.Added:
                    return EntityState.Added;
                case ObjectState.Modified:
                    return EntityState.Modified;
                case ObjectState.Deleted:
                    return EntityState.Deleted;
                default:
                    return EntityState.Unchanged;
            }
        }

        public static ObjectState ConvertState( EntityState state )
        {
            switch ( state )
            {
                case EntityState.Detached:
                    return ObjectState.UnChanged;
                case EntityState.Unchanged:
                    return ObjectState.UnChanged;
                case EntityState.Added:
                    return ObjectState.Added;
                case EntityState.Deleted:
                    return ObjectState.Deleted;
                case EntityState.Modified:
                    return ObjectState.Modified;
                default:
                    throw new ArgumentOutOfRangeException( "state" );
            }
        }
    }
}
