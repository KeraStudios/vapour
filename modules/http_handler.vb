Imports System.Web.HttpContext
Imports System.Threading

NameSpace Vapour_v1
	
	
	'========================
	'	Synchronous VB.Net Http Handler for simplity
	'========================
	Public Class vapour
		Inherits api
		Implements System.Web.IHttpHandler
		
		Public Sub ProcessRequest(ByVal http As System.Web.HttpContext) Implements System.Web.IHttpHandler.processRequest
			
			try
				
				me.http = http
				
				' Inherited from api object
				run()
				
			catch ex as exception
				
				' Inherited from api object
				failSafe(ex, http)
				
			end try
			
		End Sub
		
		Public ReadOnly Property IsReusable() As Boolean _
				Implements System.Web.IHttpHandler.IsReusable
			Get
				Return False
			End Get
		End Property
		
	End Class
	
	
	'========================
	'	Asynchronous VB.Net Http Handler for improved speed with large number of users.
	'========================
	Public Class vapourAsync
		Inherits api
		Implements IHttpAsyncHandler
		'' You'll need this to read from session
		'Implements IReadOnlySessionState 
		'' You'll need this to read/write from session
		'Implements IRequiresSessionState
		
		Private AsyncTask As AsyncTaskDelegate
		Protected Delegate Sub AsyncTaskDelegate(ByVal http As HttpContext)
		
		Public Sub ProcessRequest(ByVal http As HttpContext) Implements IHttpAsyncHandler.ProcessRequest
			
			try
				
				me.http = http
				
				' Inherited from api object
				run()
				
			catch ex as exception
				
				' Inherited from api object
				failSafe(ex, http)
				
			end try
			
		End Sub
		
		Public Function BeginProcessRequest(ByVal http As HttpContext,ByVal callback As AsyncCallback, ByVal extraData As Object) as IAsyncResult Implements IHttpAsyncHandler.BeginProcessRequest
			
			Me.AsyncTask = new AsyncTaskDelegate(Addressof ProcessRequest)
			Return AsyncTask.BeginInvoke(http,callback,extraData)
			
		End Function
		
		Public Sub EndProcessRequest(ByVal ar As IAsyncResult) Implements IHttpAsyncHandler.EndProcessRequest
			
			Me.AsyncTask.EndInvoke(ar)
			
		End Sub
		
		Public ReadOnly Property IsReusable() As Boolean Implements IHttpAsyncHandler.IsReusable
			Get
				Return False
			End Get
		End Property
		
	End Class
	
End NameSpace