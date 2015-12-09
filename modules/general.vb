NameSpace Vapour_v1
	
	Public Partial Class general
		
		
		'''''''''''''''''''''''''
		' Checks if input has a value and it not nothing
		'''''''''''''''''''''''''
		Public Shared Function isSomething(value as object) as boolean
			
			if not value is nothing and not isdbnull(value) then
				if TypeOf value is String then
					if trim(value) <> "" then
						return true
					else
						return false
					end if
				else
					return true
				end if
			else
				return false
			end if
			
		End Function
		
		
		'''''''''''''''''''''''''
		' Checks if input is nothing
		'''''''''''''''''''''''''
		Public Shared Function isNothing(value as object) as boolean
			
			if value is nothing or isdbnull(value) then
				return true
			else
				return false
			end if
			
		End Function
		
		
		'''''''''''''''''''''''''
		' Cleans string. Especially useful for database inserts/updates
		' where data needs to be cleaned up and formated.
		'''''''''''''''''''''''''
		Public Shared Function clean(value as string, Optional StringFormat as object = 0) as string
			Dim newValue as string = Replace(Trim(value), "'", "''")
			
			' Format string using VB string conversion: https://msdn.microsoft.com/en-us/library/cd7w43ec(v=vs.90).aspx
			if StringFormat <> 0 then newValue = StrConv(newValue, vbProperCase)
			
			return  newValue
			
		End Function
		
	End Class
	
End NameSpace