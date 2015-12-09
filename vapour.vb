Imports Vapour_v1.general
Imports System.Web.HttpContext
Imports Errors


Public Partial Class controllers
	
	Dim Public api
	
	Sub new(api as object)
		me.api = api
	end sub
	
End Class


NameSpace Vapour_v1
	
	'========================
	'	api - root object
	'========================
	Public Class api
		
		Dim Public controllers as controllers
		
		' Various request specific properties.
		Public params as new dictionary(of string, object)
		Public query as object
		Public body as object
		Protected baseUrl as string
		
		<ThreadStatic>
		Public Shared http as HttpContext
		
		Public debug as boolean = false
		
		ReadOnly Shared Property url as string
			Get
				return http.request.url.absolutePath
			End Get
		End Property
		
		Public Property status as integer
			Get
				return http.response.statusCode
			End Get
			Set(code as integer)
				http.response.statusCode = code
			End Set
		End Property
		
		' JSON wrapper function to simplify the conversion of objects to JSON(see display function).
		Dim Public Shared json As New System.Web.Script.Serialization.JavaScriptSerializer()
		
		' TODO: Add XML support if the user requests it using 'request.contentType'
		'Dim Public Shared xml As New System.Xml.Serialization.XmlSerializer()
		
		'''''''''''''''''''''''''
		' Initialize new api class and setup various parameters/variables.
		'''''''''''''''''''''''''
		Private Sub initialize()
			
			me.controllers = new controllers(me)
			
			query = http.request.querystring
			body = getBodyVars(http.request.form("data"))
			
			' We want the users to know that this is a JSON object, so we change the http header's "Content Type".
			http.response.ContentType = "application/json"
			
			' Needed to override the custom IIS errors which breaks the API response.
			http.response.TrySkipIisCustomErrors = True
			
			' Set initial values and get corresponding route.
			json.MaxJsonLength = Integer.MaxValue
			
		End Sub
		
		
		'''''''''''''''''''''''''
		' Gets the body/POST variables
		'''''''''''''''''''''''''
		Function getBodyVars(data as object)
			
			Dim body as object = nothing
			
			' Retreives body/POST variables.
			if isSomething(data) then
				body = json.DeserializeObject(data)
				
				' Check for bad data.
				if not TypeOf body is Dictionary(Of String,Object) then
					body = nothing
				end if
			End if
			
			return body
			
		End Function
		
		
		'''''''''''''''''''''''''
		' Loops through routes checking if they match the clients request.
		' Then cleans up and finishes the request.
		'''''''''''''''''''''''''
		Public Sub run()
			
			'''''''
			' Initialize the request.
			'''''''
			initialize()
			
			'''''''
			' Run developer created code.
			'''''''
			start()
			
			
			'''''''
			' Execute framework based on developer code.
			' TODO: Throw this into another sub.
			'''''''
			
			' Loops through route list created earlier and checks if conditions are
			' met in regards to the method and urlTemplate. And if true, then executes
			' the controller/Lambda function provided.
			Try
				Dim success as boolean = False
				For each item in routes
					if urlCheck(item.method, item.urlTemplate) then
						
						if item.cont isNot nothing then
							item.cont()
						end if
						success = True
						exit for
						
					end if
				next
				if success = false then
					
					status = 404
					display("This page doesn't exist")
					
				end if
				
			Catch ex as Exception
				
				' Stop, major error occured.
				'Dim err as new Dictionary(of String, Object)
				'err.Add("message", Ex.Message)
				'err.Add("stacktrace", Ex.StackTrace)
				errors.add(ex)
				
				display()
				
			End Try
			
			
			'''''''
			' Clean up the request.
			'''''''
			
			' Clear all cached variables.
			params = nothing
			query = nothing
			body = nothing
			Errors.clear()
			
		End Sub
		
		
		' A list of all routes created by the user which will be
		' used to deteremine and execute the correct route.
		' Note:
		'		Default OPTIONS route used for passing back header information.
		Dim routes as New list(of routeDict) _
			({new routeDict("OPTIONS", "*", nothing)})
		
		
		' NOTE: This is a special class to allow the controller/Lambda
		' function to be passed along to the routing system.
		Delegate Sub controller()
		
		
		' A custom very structured 'dictionary' that accepts the controller/Lambda
		' funtion to be used in the routes list.
		Class routeDict
			
			public property method as string
			public property urlTemplate as string
			Dim public cont as controller
			
			' Initialization of routeDict object that automatically sets everything up.
			Public Sub New(ByVal method As String,
						ByVal urlTemplate As String,
						ByVal cont As controller)
				me.method = method
				me.urlTemplate = urlTemplate
				me.cont = cont
			End Sub
			
		End Class
		
		
		'''''''''''''''''''''''''
		' Adds route to routes list to be executed by the routing system later.
		'''''''''''''''''''''''''
		Sub Route(method as string, urlTemplate as string, cont as controller)
			
			routes.Add(new routeDict(method, urlTemplate, cont))
			
		End Sub
		
		
		'''''''''''''''''''''''''
		' Returns true if client request matches the route's method and url template.
		' NOTE: Also returns url variables to be accessible later.
		'''''''''''''''''''''''''
		Protected Function urlCheck(method as string, urlTemplate as string)
			
			' Determine source of http method as the default http header method can be overridden with a POST method
			Dim methodCheck as string = http.request.HttpMethod
			
			' User overrided the method
			' TODO: remove.
			if isSomething(http.request.form("method")) then
				methodCheck = http.request.form("method")
			end if
			
			' Quick check of whether the http method matches.
			if method = methodCheck then
				
				Dim tmpBaseUrl as string = baseUrl
				if tmpBaseUrl isNot nothing then
					if tmpBaseUrl(tmpBaseUrl.length-1) = "/" then
						tmpBaseUrl = tmpBaseUrl.substring(0, tmpBaseUrl.length-1)
					end if
				end if
				
				' NOTE: The '^' makes sure the route regex starts at the beginning of the 'url'.
				urlTemplate = "^" & tmpBaseUrl & urlTemplate & "$"
				
				' Sets up and matches the regular expression using the url template.
				Dim urlReg = New Regex(urlTemplate)
				Dim urlMatch = urlReg.Match(url)
				
				' Checks if url is a match to the urlTemplate
				if urlMatch.success then
					
					' Get the url variables.
					' Note: this is not to be confused with querystring variables; although they work the similar.
					Dim names() as string = urlReg.GetGroupNames()
					for each name in names
						Dim group as Group = urlMatch.groups.Item(name)
						params.Add(name, group.value)
					Next
					
					return true ' Method and url template are a match.
				end if
			end	if
			
			' Fall back
			return false ' Method or Url template didn't match.
			
		End Function
		
		
		'''''''''''''''''''''''''
		' Send response back to client
		'''''''''''''''''''''''''
		Public Sub display(optional data as Object = nothing)
			
			Dim result As New Dictionary(Of String, Object)
			result.Add("status", status)
			result.Add("errors", errors.all(debug))
			result.Add("data", data)
			
			' Return all POSTed values only if there was an error
			if status <> 200 and debug = true then
				
				result.Add("inputValues", body)
				result.Add("url", url)
				
				if isSomething(http.request.form("method"))
					result.Add("method", http.request.form("method"))
				else
					result.Add("method", http.request.HttpMethod)
				end if
				
			end if
			
			
			http.response.write(json.Serialize(result))
			
			' TODO: Add XML support if the user requests it using 'request.contentType'
			'if lcase(request.contentType).Contains("xml") then
			'	'res = xml.Serialize(result)
			'else
			'	res = json.Serialize(result)
			'end if
			
			logging(result)
			
		End Sub
		
		
		'''''''''''''''''''''''''
		' Logs request and response on user.
		'''''''''''''''''''''''''
		Overridable Public Sub logging(clientResponse as object)
			
		End Sub
		
		
		'''''''''''''''''''''''''
		' Is overrided by developer to build the api.
		'''''''''''''''''''''''''
		Overridable Public Sub start()
			
		End Sub
		
		
		'''''''''''''''''''''''''
		' This fail safe subroutine should be fully self
		' contained so if an error occurs that manages
		' to circumvent the interal control structures
		' it won't complete crash everything.
		'''''''''''''''''''''''''
		Protected Sub failSafe(ex as exception, http as HttpContext)
			
			Dim json As New System.Web.Script.Serialization.JavaScriptSerializer()
			
			Dim err As New Dictionary(Of String, Object)
			err.add("status", 500)
			err.add("message", ex.message)
			err.add("stacktrace", ex.stacktrace)
			
			Dim result As New Dictionary(Of String, Object)
			result.Add("status", 500)
			result.Add("errors", err)
			result.Add("data", nothing)
			
			http.response.ContentType = "application/json"
			
			http.response.write(json.serialize(result))
			
		End Sub
		
	End Class
	
End NameSpace