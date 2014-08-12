Imports System.IO
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Threading
Imports System.ComponentModel

Imports Microsoft.Win32
Imports System.Management
Imports System.ServiceProcess

Public Class Main_Screen

    Private dontsave As Boolean = False

    Private busyworking As Boolean = False


    Private Sub Error_Handler(ByVal ex As Exception, Optional ByVal identifier_msg As String = "")
        Try
            If ex.Message.IndexOf("Thread was being aborted") < 0 Then
                Dim Display_Message1 As New Display_Message()
                Display_Message1.Message_Textbox.Text = "The Application encountered the following problem: " & vbCrLf & identifier_msg & ":" & ex.Message.ToString

                Display_Message1.Timer1.Interval = 1000
                Display_Message1.ShowDialog()
                Dim dir As System.IO.DirectoryInfo = New System.IO.DirectoryInfo((Application.StartupPath & "\").Replace("\\", "\") & "Error Logs")
                If dir.Exists = False Then
                    dir.Create()
                End If
                dir = Nothing
                Dim filewriter As System.IO.StreamWriter = New System.IO.StreamWriter((Application.StartupPath & "\").Replace("\\", "\") & "Error Logs\" & Format(Now(), "yyyyMMdd") & "_Error_Log.txt", True)
                filewriter.WriteLine("#" & Format(Now(), "dd/MM/yyyy hh:mm:ss tt") & " - " & identifier_msg & ":" & ex.ToString)
                filewriter.Flush()
                filewriter.Close()
                filewriter = Nothing
            End If
            ex = Nothing
            identifier_msg = Nothing
        Catch exc As Exception
            MsgBox("An error occurred in the application's error handling routine. The application will try to recover from this serious error.", MsgBoxStyle.Critical, "Critical Error Encountered")
        End Try
    End Sub


    ' This event handler is where the actual work is done.
    Private Sub backgroundWorker1_DoWork(ByVal sender As Object, ByVal e As DoWorkEventArgs) Handles BackgroundWorker1.DoWork

        ' Get the BackgroundWorker object that raised this event.
        Dim worker As BackgroundWorker = CType(sender, BackgroundWorker)

        ' Assign the result of the computation
        ' to the Result property of the DoWorkEventArgs
        ' object. This is will be available to the 
        ' RunWorkerCompleted eventhandler.
        e.Result = MainWorkerFunction(worker, e)
        sender = Nothing
        e = Nothing
        worker.Dispose()
        worker = Nothing
    End Sub 'backgroundWorker1_DoWork

    ' This event handler deals with the results of the
    ' background operation.
    Private Sub backgroundWorker1_RunWorkerCompleted(ByVal sender As Object, ByVal e As RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted
        busyworking = False
        ' First, handle the case where an exception was thrown.
        If Not (e.Error Is Nothing) Then
            Error_Handler(e.Error, "backgroundWorker1_RunWorkerCompleted")
        ElseIf e.Cancelled Then
            ' Next, handle the case where the user canceled the 
            ' operation.
            ' Note that due to a race condition in 
            ' the DoWork event handler, the Cancelled
            ' flag may not have been set, even though
            ' CancelAsync was called.

        Else
            ' Finally, handle the case where the operation succeeded.

        End If


        sender = Nothing
        e = Nothing


    End Sub 'backgroundWorker1_RunWorkerCompleted

    Private Sub backgroundWorker1_ProgressChanged(ByVal sender As Object, ByVal e As ProgressChangedEventArgs) Handles BackgroundWorker1.ProgressChanged

        sender = Nothing
        e = Nothing
    End Sub

    Function MainWorkerFunction(ByVal worker As BackgroundWorker, ByVal e As DoWorkEventArgs) As Boolean
        Dim result As Boolean = False
        Try

            If worker.CancellationPending Then
                e.Cancel = True
                Return False
            End If

            WebBrowser1.Navigate("http://www.commerce.uct.ac.za/services/newsfeed/rules.htm")
            WebBrowser1.AllowNavigation = False
            WebBrowser2.Navigate("http://www.commerce.uct.ac.za/services/newsfeed/")
            WebBrowser2.AllowNavigation = False


        Catch ex As Exception
            Error_Handler(ex, "MainWorkerFunction")
        End Try
        worker.Dispose()
        worker = Nothing
        e = Nothing
        Return result

    End Function

    Private Sub Form1_Close(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles MyBase.FormClosed
        Try
            If Not dontsave = True Then

                Try
                    My.Computer.Network.DownloadFile("http://www.commerce.uct.ac.za/Services/Newsfeed/rules.htm", (Application.StartupPath & "\Lab Rules.htm").Replace("\\", "\"), "", "", False, 30000, True)
                    'My.Computer.Network.DownloadFile("http://www.commerce.uct.ac.za/Services/Newsfeed/rules.htm", "F:\Lab Rules.htm", "", "", False, 30000, True)
                    If My.Computer.FileSystem.DirectoryExists("f:\") = True Then
                        Try
                            My.Computer.FileSystem.MoveFile((Application.StartupPath & "\Lab Rules.htm").Replace("\\", "\"), "F:\Lab Rules.htm", True)
                        Catch ex As Exception

                        End Try
                    End If
                Catch ex As Exception
                    Error_Handler(ex, "Saving Rules.htm")
                End Try
            End If

            '-----------------------------------------------
            'Lab Usage Tracker
            '-----------------------------------------------
            Dim currentuser As String = ""
            currentuser = ReturnRegKeyValue("HKEY_CURRENT_USER", "Volatile Environment", "NWUSERNAME")
            If currentuser.StartsWith("Fail.") = True Or currentuser.StartsWith("Failure") = True Then
                currentuser = "UNKNOWN"
            End If
            Dim machinename, ipaddress, macaddress As String
            machinename = ""
            ipaddress = ""
            macaddress = ""
            Try
                machinename = Environment.MachineName
            Catch ex As Exception
                Error_Handler(ex, "Retrieve machine name")
            End Try
            Try
                Dim mc As System.Management.ManagementClass
                Dim mo As ManagementObject
                mc = New ManagementClass("Win32_NetworkAdapterConfiguration")
                Dim moc As ManagementObjectCollection = mc.GetInstances()
                For Each mo In moc
                    If mo.Item("IPEnabled") = True Then
                        macaddress = mo.Item("MacAddress").ToString()
                        Dim addresses() As String = CType(mo("IPAddress"), String())
                        Dim subnets() As String = CType(mo("IPSubnet"), String())
                        Dim s As String
                        Dim a As Integer = 0
                        For Each s In addresses
                            ipaddress = s.ToString()
                            a += 1
                            If a = 1 Then
                                Exit For
                            End If
                        Next
                    End If
                Next
            Catch ex As Exception
                Error_Handler(ex, "Retrieve MAC and IP addresses")
            End Try
            Dim logdate As String = Format(Now, "yyyyMMddHHmmss")
            If My.Computer.Network.IsAvailable = True Then
                Try
                    My.Computer.Network.DownloadFile("http://www.commerce.uct.ac.za/Services/Lab Usage Tracker/Submit.asp?Page_Action=create&Novell_Account=" & currentuser & "&Time_Stamp=" & logdate & "&IP_Address=" & ipaddress & "&MAC_Address=" & macaddress & "&Machine_Name=" & machinename & "", (My.Computer.FileSystem.SpecialDirectories.ProgramFiles & "\ReportResult.htm").Replace("\\", "\"), "", "", False, 100000, True)
                    If My.Computer.FileSystem.FileExists((My.Computer.FileSystem.SpecialDirectories.ProgramFiles & "\ReportResult.htm").Replace("\\", "\")) = True Then
                        My.Computer.FileSystem.DeleteFile((My.Computer.FileSystem.SpecialDirectories.ProgramFiles & "\ReportResult.htm").Replace("\\", "\"))
                    End If

                Catch ex As Exception
                    Error_Handler(ex, "Logging Kill to OBE1")
                End Try
            End If
            '-----------------------------------------------

        Catch ex As Exception
            Error_Handler(ex, "Application Close")
        End Try
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Try
            Control.CheckForIllegalCrossThreadCalls = False
            Me.Text = "Login Rules " & My.Application.Info.Version.Major & Format(My.Application.Info.Version.Minor, "00") & Format(My.Application.Info.Version.Build, "00") & "." & My.Application.Info.Version.Revision
            Me.Width = Screen.PrimaryScreen.Bounds.Width
            Me.Height = Screen.PrimaryScreen.Bounds.Height
            Me.Top = 0
            Me.Left = 0
            Me.SplitContainer1.SplitterDistance = Me.Width - 550
            If My.Computer.FileSystem.FileExists((Application.StartupPath & "\Loading.html").Replace("\\", "\")) Then
                Me.WebBrowser2.Navigate((Application.StartupPath & "\Loading.html").Replace("\\", "\"))
                Me.WebBrowser1.Navigate((Application.StartupPath & "\Loading.html").Replace("\\", "\"))
            End If

            Button1.Select()
            Button1.Focus()

            If busyworking = False Then
                busyworking = True
                ' Start the asynchronous operation.
                BackgroundWorker1.RunWorkerAsync()
            End If

        Catch ex As Exception
            Error_Handler(ex, "Application Load")
        End Try

    End Sub


    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Me.Hide()
        Me.Close()
    End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        Try
            dontsave = True
            Me.Hide()
            Dim ProcID As Integer
            ProcID = Shell("shutdown -l -f", AppWinStyle.NormalFocus, True, 30000)
            Me.Close()
        Catch ex As Exception
            Error_Handler(ex, "Do not accept Rules")
        End Try
    End Sub

    Public Function ReturnRegKeyValue(ByVal MainKey As String, ByVal RequestedKey As String, ByVal Value As String) As String
        Dim result As String = "Fail."
        Try
            Dim oReg As RegistryKey
            Dim regkey As RegistryKey
            Try
                Select Case MainKey.ToUpper
                    Case "HKEY_CURRENT_USER"
                        oReg = Registry.CurrentUser
                    Case "HKEY_CLASSES_ROOT"
                        oReg = Registry.ClassesRoot
                    Case "HKEY_LOCAL_MACHINE"
                        oReg = Registry.LocalMachine
                    Case "HKEY_USERS"
                        oReg = Registry.Users
                    Case "HKEY_CURRENT_CONFIG"
                        oReg = Registry.CurrentConfig
                    Case Else
                        oReg = Registry.LocalMachine
                End Select

                regkey = oReg
                oReg.Close()
                If RequestedKey.EndsWith("\") = True Then
                    RequestedKey = RequestedKey.Remove(RequestedKey.Length - 1, 1)
                End If
                Dim subs() As String = (RequestedKey).Split("\")

                Dim doContinue As Boolean = True
                For Each stri As String In subs
                    If doContinue = False Then
                        Exit For
                    End If
                    If regkey Is Nothing = False Then
                        Dim skn As String() = regkey.GetSubKeyNames()
                        Dim strin As String

                        doContinue = False
                        For Each strin In skn
                            If stri = strin Then
                                regkey = regkey.OpenSubKey(stri, True)
                                doContinue = True
                                Exit For
                            End If
                        Next
                    End If
                Next
                If doContinue = True Then
                    If regkey Is Nothing = False Then
                        Dim str As String() = regkey.GetValueNames()
                        Dim val As String
                        Dim foundit As Boolean = False
                        For Each val In str
                            If Value = val Then
                                foundit = True
                                result = regkey.GetValue(Value)
                                Exit For
                            End If
                        Next
                        If foundit = False Then
                            result = "Fail. Could not locate Value within Registry Key"
                        End If
                        regkey.Close()
                    End If
                Else
                    result = "Fail. Key cannot be located"
                End If
            Catch ex As Exception
                Error_Handler(ex, "ReturnRegKeyValue")
                result = "Fail. Check Error Log for further details"
            End Try
        Catch ex As Exception
            Error_Handler(ex, "ReturnRegKeyValue")
            result = "Fail. Check Error Log for further details"
        End Try
        Return result
    End Function
End Class
