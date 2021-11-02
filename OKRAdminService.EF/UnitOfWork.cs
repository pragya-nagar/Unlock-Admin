using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OKRAdminService.EF
{
    public sealed class UnitOfWork : IUnitOfWorkAsync
    {
        private Dictionary<string, dynamic> repositories;
        private bool disposed;

        public IDataContextAsync DataContext { get; private set; }

        public UnitOfWork(IDataContextAsync _context, IOperationStatus _opStatus)
        {
            DataContext = _context;
            repositories = new Dictionary<string, dynamic>();
        }

        public IOperationStatus SaveChanges()
        {
            IOperationStatus opStatus = new OperationStatus { Success = false };
            try
            {
                int numRec = DataContext.SaveChanges();
                opStatus.Success = true;
                opStatus.RecordsAffected = numRec;
            }
            catch (SystemException ex)
            {
                opStatus = opStatus.CreateFromException(ex);
            }
            return opStatus;
        }
        public void Dispose()
        {
            DisposeIt(true);
            GC.SuppressFinalize(this);
        }

        public void DisposeIt(bool disposing)
        {
            if (disposed)
                return;

            if (disposing && DataContext != null)
            {
                DataContext.Dispose();
                DataContext = null;
            }
            disposed = true;
        }
        public IRepository<T> Repository<T>() where T : class, IObjectState
        {
            return RepositoryAsync<T>();
        }

        public async Task<IOperationStatus> SaveChangesAsync()
        {
            IOperationStatus opStatus = new OperationStatus { Success = false };
            try
            {
                int numRec = await DataContext.SaveChangesAsync();
                opStatus.Success = true;
                opStatus.Message = "Record successfully saved!";
                opStatus.RecordsAffected = numRec;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var message = $"The object is already modified. Please refresh the container before updating the object again.";
                opStatus = opStatus.CreateFromException(ex);
                opStatus.Message = message;
            }
            catch (SystemException ex)
            {
                opStatus = opStatus.CreateFromException(ex);

                if (!string.IsNullOrWhiteSpace(opStatus.InnerInnerMessage))
                    opStatus.Message = opStatus.Message + $"{Environment.NewLine} Exception: {opStatus.InnerInnerMessage}";
            }
            return opStatus;
        }

        /// <summary>
        ///  on using nameof(T) always gives same name.therefore repositories.ContainsKey( type ) return true if there is any entry in dictionary.
        ///  this was causing error in creating multiple repository. now fixed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IRepositoryAsync<T> RepositoryAsync<T>() where T : class, IObjectState
        {
            if (repositories == null)
                repositories = new Dictionary<string, dynamic>();

            var type = typeof(T).Name;

            if (repositories.ContainsKey(type))
                return (IRepositoryAsync<T>)repositories[type];

            var repositoryType = typeof(Repository<>);
            repositories.Add(type, Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), DataContext));

            return repositories[type];
        }
        public bool Commit()
        {
            return true;
        }

        public void Rollback()
        {
            DataContext.SyncObjectStatePostCommit();
        }
    }

}
