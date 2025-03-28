using System;

namespace Kogase.Models
{
    public class KogasePromise<TOkType, TErrorType>
    {
        Action<TOkType> okDelegates;
        Action<TErrorType> errorDelegates;
        Action finalDelegates;
        TOkType queuedOkObject;
        TErrorType queuedErrorObject;
        
        bool isOk;
        bool isError;
        
        internal void Resolve(TOkType okObject)
        {
            if (isOk || isError) return;
            queuedOkObject = okObject;
            isOk = true;
    
            okDelegates?.Invoke(okObject);
            finalDelegates?.Invoke();
        }

        internal void Reject(TErrorType errorObject)
        {
            if (isOk || isError) return;
            queuedErrorObject = errorObject;
            isError = true;
    
            errorDelegates?.Invoke(errorObject);
            finalDelegates?.Invoke();
        }
        
        public KogasePromise<TOkType, TErrorType> Then(Action<TOkType> okDelegate)
        {
            if (okDelegate == null) return this;
            
            okDelegates += okDelegate;
            if (isOk)
            {
                okDelegate(queuedOkObject);
            }
            return this;
        }
        
        public KogasePromise<TOkType, TErrorType> Catch(Action<TErrorType> errorDelegate)
        {
            if (errorDelegate == null) return this;
            
            errorDelegates += errorDelegate;
            if (isError)
            {
                errorDelegate(queuedErrorObject);
            }
            return this;
        }
        
        public KogasePromise<TOkType, TErrorType> Finally(Action finalDelegate)
        {
            if (finalDelegate == null) return this;
            
            finalDelegates += finalDelegate;
            if (isOk || isError)
            {
                finalDelegate();
            }
            return this;
        }
    }

    public class KogasePromise<TErrorType>
    {
        Action okDelegates;
        Action<TErrorType> errorDelegates;
        Action finalDelegates;
        TErrorType queuedErrorObject;
        
        bool isOk;
        bool isError;
        
        internal void Resolve()
        {
            isOk = true;
            
            okDelegates?.Invoke();
            finalDelegates?.Invoke();
        }
        
        internal void Reject(TErrorType errorObject)
        {
            queuedErrorObject = errorObject;
            isError = true;
            
            errorDelegates?.Invoke(errorObject);
            finalDelegates?.Invoke();
        }
        
        public KogasePromise<TErrorType> Then(Action okDelegate)
        {
            if (okDelegate == null) return this;
            
            okDelegates += okDelegate;
            if (isOk)
            {
                okDelegate();
            }
            return this;
        }
        
        public KogasePromise<TErrorType> Catch(Action<TErrorType> errorDelegate)
        {
            if (errorDelegate == null) return this;
            
            errorDelegates += errorDelegate;
            if (isError)
            {
                errorDelegate(queuedErrorObject);
            }
            return this;
        }
        
        public KogasePromise<TErrorType> Finally(Action finalDelegate)
        {
            if (finalDelegate == null) return this;
            
            finalDelegates += finalDelegate;
            if (isOk || isError)
            {
                finalDelegate();
            }
            return this;
        }
    }
}