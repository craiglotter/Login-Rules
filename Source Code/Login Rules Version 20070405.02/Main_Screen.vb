Imports System.IO
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Threading
Imports System.ComponentModel

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
                If My.Computer.FileSystem.DirectoryExists("f:\") Then
                    My.Computer.Network.DownloadFile("http://www.commerce.uct.ac.za/Services/Newsfeed/rules.htm", "F:\Lab Rules.htm", "", "", False, 30000, True)
                End If
            End If
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
End Class
