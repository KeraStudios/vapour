NameSpace Vapour_v1
	
	
	'''''''''''''''''''''''''
	' Vapour Errors class that takes care
	' of HttpContext.Current error handler.
	' Note:
	' 		HttpContext.Current is tricky when
	' 		dealing with threading. Therefore it
	' 		must me relative to base Vapour_v#.api
	' 		class.
	'''''''''''''''''''''''''
	Public Class Errors
		
		'<ThreadStatic> Public Shared http as HttpContext = api.http
		
		Shared Public Sub add(err as exception)
			api.http.addError(err)
		End Sub
		
		Shared Public Function all(optional debug as boolean = false)
			
		
			
			Dim tmpErrors as new list(of object)
			
			'TODO
			'try
				if api.http.AllErrors isnot nothing then
					
					for each err as object in api.http.AllErrors
						
						Dim tmpErr as new dictionary(of string, object)
						
						if err.getType.getProperty("status") isNot Nothing then
							tmpErr.add("status", err.status)
						end if
						
						tmpErr.add("message", err.message)
						
						if err.getType.getProperty("data") isNot Nothing then
							if err.data.count > 0 then
								tmpErr.add("data", err.data)
							end if
						end if
						
						if debug = true then
							if err.getType.getProperty("stacktrace") isNot Nothing then
								tmpErr.add("stacktrace", err.stacktrace)
							else
								tmpErr.add("stacktrace", err.tostring())
							end if
						end if
						
						tmpErrors.add(tmpErr)
						
					next
					
					return tmpErrors
				else
					return nothing
				end if
				
			'catch ex as exception
			'	
			'	Dim exTest as new exception
			'	api.http.addError(exTest)
			'	return api.http.AllErrors
			'	
			'end Try
			
		End Function
		
		Shared Public Sub clear
			api.http.clearError()
		End Sub
		
	End Class
	
	
	'''''''''''''''''''''''''
	' Base Vapour Exceptions
	'''''''''''''''''''''''''
	Public Class VapourException
		Inherits SystemException
		
		'<ThreadStatic> Public Shared http as HttpContext = api.http
		
		Protected _status as integer = 500
		Property status as integer
			Get
				return _status
			End Get
			Protected Set(code as integer)
				_status = code
			End Set
		End Property
		
		Public Sub New(ByVal status as integer, ByVal exMsg as string)
			MyBase.New(exMsg)
			_status = status
			
			' If this error's status code is worse then
			' the current response status then escalate
			' the level.
			if api.http.response.statusCode < status then
				api.http.response.statusCode = status
			end if
		End Sub
	End Class
	
	
	'''''''''''''''''''''''''
	' Vapour API Exceptions
	'''''''''''''''''''''''''
	
	'''''''''''''''''''''''''
	' HTTP 404 - Not Found Error
	'''''''''''''''''''''''''
	Public Class Http404
		Inherits VapourException
		
		Public Sub New()
			MyBase.New(404, "404 - Not Found")
		End Sub
		
		Public Sub New(message as string)
			MyBase.New(404, message)
		End Sub
		
	End Class
	
	'''''''''''''''''''''''''
	' HTTP 400 - User Error
	'''''''''''''''''''''''''
	Public Class Http400
		Inherits VapourException
		
		Public Sub New()
			MyBase.New(400, "400 - User Error")
		End Sub
		
		Public Sub New(message as string)
			MyBase.New(400, message)
		End Sub
		
	End Class
	
	
	'''''''''''''''''''''''''
	' HTTP 500 - Server Error
	'''''''''''''''''''''''''
	Public Class Http500
		Inherits VapourException
		
		Public Sub New()
			MyBase.New(500, "500 - Not Found")
		End Sub
		
		Public Sub New(message as string)
			MyBase.New(500, message)
		End Sub
		
	End Class
	
End NameSpace