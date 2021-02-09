'MIT License

'Copyright(c) 2021 HBSnail

'Permission Is hereby granted, free Of charge, to any person obtaining a copy
'of this software And associated documentation files (the "Software"), to deal
'in the Software without restriction, including without limitation the rights
'to use, copy, modify, merge, publish, distribute, sublicense, And/Or sell
'copies of the Software, And to permit persons to whom the Software Is
'furnished to do so, subject to the following conditions:

'The above copyright notice And this permission notice shall be included In all
'copies Or substantial portions of the Software.

'THE SOFTWARE Is PROVIDED "AS IS", WITHOUT WARRANTY Of ANY KIND, EXPRESS Or
'IMPLIED, INCLUDING BUT Not LIMITED To THE WARRANTIES Of MERCHANTABILITY,
'FITNESS FOR A PARTICULAR PURPOSE And NONINFRINGEMENT. IN NO EVENT SHALL THE
'AUTHORS Or COPYRIGHT HOLDERS BE LIABLE For ANY CLAIM, DAMAGES Or OTHER
'LIABILITY, WHETHER In AN ACTION Of CONTRACT, TORT Or OTHERWISE, ARISING FROM,
'OUT OF Or IN CONNECTION WITH THE SOFTWARE Or THE USE Or OTHER DEALINGS IN THE
'SOFTWARE.

Imports System.Net
Imports System.Text
Imports Newtonsoft.Json.Linq
Imports System.IO
Imports System.ComponentModel
Imports QRCoder
Imports System.IO.Compression
Imports System.Threading

Public Class Form1

    Private Cookie As String = ""
    Private oauthKey As String = ""
    Private defaultqrcode As Bitmap
    Delegate Sub setState(q As String, t As String, p As Integer)

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        TreeView1.Nodes.Clear()
        Try

            Dim videourl As String = TextBox1.Text.Split("?")(0).Replace("\", "/")
            Dim congs As String() = videourl.Split("/")
            Dim vid As String = congs(congs.Length - 1)
            If vid = "" Then
                vid = congs(congs.Length - 2)
            End If
            Dim client As New WebClientEx
            client.Timeout = 5000
            client.Headers.Add(HttpRequestHeader.Cookie, Cookie)
            client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88")
            client.Encoding = Encoding.UTF8
            Dim videoparts As New List(Of Vpartsinfo)
            Dim title As String
            Dim picurl As String
            If vid.Substring(0, 2).ToLower = "av" Or vid.Substring(0, 2).ToLower = "bv" Then
                Dim videoinfo As String = client.DownloadString("http://api.bilibili.com/x/web-interface/view?" +
                                                                If(vid.Substring(0, 2).ToLower = "av", "a", "bv") + "id=" +
                                                                If(vid.Substring(0, 2).ToLower = "av", vid.Substring(2, vid.Length - 2), vid))
                Dim jo As JObject = JObject.Parse(videoinfo)
                title = jo.SelectToken("data").SelectToken("title").ToString()
                picurl = jo.SelectToken("data").SelectToken("pic").ToString()
                Dim pagesjson As JToken = jo.SelectToken("data").SelectToken("pages")
                For i = 0 To pagesjson.Count - 1
                    Dim vi As New Vpartsinfo
                    vi.cid = pagesjson(i).SelectToken("cid").ToString
                    vi.partsname = pagesjson(i).SelectToken("part").ToString
                    vi.abvid = vid
                    videoparts.Add(vi)
                Next

            ElseIf vid.Substring(0, 2).ToLower = "ep" Or vid.Substring(0, 2).ToLower = "ss" Then
                Dim videoinfo As String = client.DownloadString("https://api.bilibili.com/pgc/view/web/season?" +
                                                               If(vid.Substring(0, 2).ToLower = "ep", "ep_", "season_") + "id=" +
                                                                vid.Substring(2, vid.Length - 2))
                Dim jo As JObject = JObject.Parse(videoinfo)
                Dim allep As JToken = jo.SelectToken("result").SelectToken("episodes")
                title = jo.SelectToken("result").SelectToken("series").SelectToken("series_title").ToString()
                For i = 0 To allep.Count - 1
                    Dim bvid, cid, sname As String
                    Dim vi As New Vpartsinfo
                    bvid = allep(i).SelectToken("bvid")
                    cid = allep(i).SelectToken("cid")
                    sname = allep(i).SelectToken("share_copy")
                    vi.cid = cid
                    vi.abvid = bvid
                    vi.partsname = sname
                    videoparts.Add(vi)
                Next

            End If



            Dim qn As String = "80"
            Select Case ComboBox1.Text
                Case "720P60 高清"
                    qn = "74"
                    Exit Select
                Case "1080P60 高清"
                    qn = "116"
                    Exit Select
                Case "4K 超清"
                    qn = "120"
                    Exit Select
                Case "1080P+ 高清"
                    qn = "112"
                    Exit Select
                Case "1080P 高清"
                    qn = "80"
                    Exit Select
                Case "720P 高清"
                    qn = "64"
                    Exit Select
                Case "480P 清晰"
                    qn = "32"
                    Exit Select
                Case "360P 流畅"
                    qn = "16"
                    Exit Select
                Case Else
                    qn = "80"
                    Exit Select
            End Select
            For i = 0 To videoparts.Count - 1
                Try

                    If RadioButton2.Checked And RadioButton1.Checked = False Then
                        Dim cidinfo As String = client.DownloadString("https://api.bilibili.com/x/player/playurl?cid=" +
                                                                      videoparts(i).cid +
                                                                      "&otype=json&fourk=1&" +
                                                                      videoparts(i).abvid.Substring(0, 2).ToLower + "id=" +
                                                                      If(videoparts(i).abvid.Substring(0, 2).ToLower = "av", videoparts(i).abvid.Substring(2, videoparts(i).abvid.Length - 2), videoparts(i).abvid) +
                                                                      "&qn=" + qn)
                        Dim cjsonobj As JObject = JObject.Parse(cidinfo)
                        videoparts(i).setQuality(cjsonobj.SelectToken("data").SelectToken("quality").ToString())
                        Dim videopartjson As JToken = cjsonobj.SelectToken("data").SelectToken("durl")
                        videoparts(i).Additional.Add("弹幕: ")
                        videoparts(i).urls.Add("https://comment.bilibili.com/" + videoparts(i).cid + ".xml")
                        For j = 0 To videopartjson.Count - 1
                            videoparts(i).Additional.Add(CLng(videopartjson(j).SelectToken("size").ToString()).ToString)
                            videoparts(i).urls.Add(videopartjson(j).SelectToken("url").ToString())
                        Next

                    ElseIf RadioButton1.Checked And RadioButton2.Checked = False Then
                        Dim cidinfo As String = client.DownloadString("https://api.bilibili.com/x/player/playurl?cid=" +
                                                                      videoparts(i).cid +
                                                                      "&fnver=0&fnval=80&fourk=1&otype=json&" +
                                                                      videoparts(i).abvid.Substring(0, 2).ToLower + "id=" +
                                                                      If(videoparts(i).abvid.Substring(0, 2).ToLower = "av", videoparts(i).abvid.Substring(2, videoparts(i).abvid.Length - 2), videoparts(i).abvid) +
                                                                        "&qn=" + qn)
                        Dim cjsonobj As JObject = JObject.Parse(cidinfo)
                        Dim DASHVideoParts As JToken = cjsonobj.SelectToken("data").SelectToken("dash").SelectToken("video")
                        videoparts(i).Additional.Add("弹幕: ")
                        videoparts(i).urls.Add("https://comment.bilibili.com/" + videoparts(i).cid + ".xml")

                        For j = 0 To DASHVideoParts.Count - 1
                            Dim backupDASHVideoParts As JToken = DASHVideoParts(j).SelectToken("backupUrl")
                            videoparts(i).Additional.Add("视频:" + convertqual2string(DASHVideoParts(j).SelectToken("id").ToString()) + " ")
                            videoparts(i).urls.Add(DASHVideoParts(j).SelectToken("baseUrl").ToString())
                            For k = 0 To backupDASHVideoParts.Count - 1
                                videoparts(i).Additional.Add("视频:" + convertqual2string(DASHVideoParts(j).SelectToken("id").ToString()) + " ")
                                videoparts(i).urls.Add(backupDASHVideoParts(k).ToString)
                            Next
                        Next

                        Dim DASHAudioParts As JToken = cjsonobj.SelectToken("data").SelectToken("dash").SelectToken("audio")
                        For j = 0 To DASHAudioParts.Count - 1
                            Dim backupDASHAideoParts As JToken = DASHAudioParts(j).SelectToken("backupUrl")
                            videoparts(i).Additional.Add("音频:" + convertqual2string(DASHAudioParts(j).SelectToken("id").ToString()) + " ")
                            videoparts(i).urls.Add(DASHAudioParts(j).SelectToken("baseUrl").ToString())
                            For k = 0 To backupDASHAideoParts.Count - 1
                                videoparts(i).Additional.Add("视频:" + convertqual2string(DASHAudioParts(j).SelectToken("id").ToString()) + " ")
                                videoparts(i).urls.Add(backupDASHAideoParts(k).ToString)
                            Next
                        Next
                    End If
                Catch ex As Exception

                End Try
            Next
            Dim tree As New TreeNode()
            tree.Text = title
            tree.Tag = picurl
            For i = 0 To videoparts.Count - 1
                Dim treen As New TreeNode()
                treen.Text = videoparts(i).partsname + " " + videoparts(i).getQuality()
                treen.Tag = videoparts(i).cid
                For j = 0 To videoparts(i).urls.Count - 1
                    Dim t3 As New TreeNode
                    With t3
                        .Tag = videoparts(i).urls(j)
                        .Text = " " + If(videoparts(i).Additional(j) Like "#*", "大小:" + FormatBytes(videoparts(i).Additional(j)) & " ", videoparts(i).Additional(j)) & " " + videoparts(i).urls(j)
                    End With
                    treen.Nodes.Add(t3)
                Next
                tree.Nodes.Add(treen)
            Next
            TreeView1.Nodes.Add(tree)
            TreeView1.ExpandAll()

        Catch ex As Exception
            MsgBox("解析失败: " & ex.Message)
        End Try
    End Sub

    Private Function convertqual2string(v As String) As String
        Select Case v
            Case "30216"
                Return "64K"
                Exit Select
            Case "30232"
                Return "132K"
                Exit Select
            Case "30280"
                Return "192K"
                Exit Select
            Case "74"
                Return "720P60 高清"
                Exit Select
            Case "116"
                Return "1080P60 高清"
                Exit Select
            Case "120"
                Return "4K 超清"
                Exit Select
            Case "112"
                Return "1080P+ 高清"
                Exit Select
            Case "80"
                Return "1080P 高清"
                Exit Select
            Case "64"
                Return "720P 高清"
                Exit Select
            Case "32"
                Return "480P 清晰"
                Exit Select
            Case "16"
                Return "360P 流畅"
                Exit Select
            Case Else
                Return ""
                Exit Select
        End Select
    End Function

    Public Function FormatBytes(ByVal BytesCaller As ULong) As String
        Dim DoubleBytes As Double
        Try
            Select Case BytesCaller
                Case Is >= 1099511627776
                    DoubleBytes = CDbl(BytesCaller / 1099511627776)
                    Return FormatNumber(DoubleBytes, 2) & " TB"
                Case 1073741824 To 1099511627775
                    DoubleBytes = CDbl(BytesCaller / 1073741824)
                    Return FormatNumber(DoubleBytes, 2) & " GB"
                Case 1048576 To 1073741823
                    DoubleBytes = CDbl(BytesCaller / 1048576)
                    Return FormatNumber(DoubleBytes, 2) & " MB"
                Case 1024 To 1048575
                    DoubleBytes = CDbl(BytesCaller / 1024)
                    Return FormatNumber(DoubleBytes, 2) & " KB"
                Case 0 To 1023
                    DoubleBytes = BytesCaller
                    Return FormatNumber(DoubleBytes, 2) & " bytes"
                Case Else
                    Return ""
            End Select
        Catch
            Return ""
        End Try

    End Function

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        ThreadPool.QueueUserWorkItem(AddressOf addQueueDownload)
        ComboBox1.Text = "4K 超清"
        Dim p As New ProgressBar
        p.Maximum = 101
        p.Value = 100
        defaultqrcode = PictureBox1.Image
    End Sub

    Private Sub TreeView1_AfterCheck(sender As Object, e As TreeViewEventArgs) Handles TreeView1.AfterCheck
        If e.Action <> TreeViewAction.Unknown Then
            CheckAllChildNodes(e.Node, e.Node.Checked)
            Dim bol As Boolean = True

            If e.Node.Parent IsNot Nothing Then

                For i As Integer = 0 To e.Node.Parent.Nodes.Count - 1
                    If Not e.Node.Parent.Nodes(i).Checked Then bol = False
                Next

                e.Node.Parent.Checked = bol
            End If
        End If
    End Sub

    Public Sub CheckAllChildNodes(ByVal treeNode As TreeNode, ByVal nodeChecked As Boolean)
        For Each node As TreeNode In treeNode.Nodes
            node.Checked = nodeChecked

            If node.Nodes.Count > 0 Then
                Me.CheckAllChildNodes(node, nodeChecked)
            End If
        Next
    End Sub

    Structure dfinfo
        Public url As String
        Public path As String
    End Structure
    Dim dq As New Queue(Of dfinfo)
    WithEvents wc As WebClientEx
    Dim lock As Boolean = False
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click

        CheckForAllNodes(TreeView1.Nodes)

    End Sub

    Private Sub addQueueDownload(state As Object)
        While (True)
            If lock = False AndAlso dq.Count > 0 Then
                lock = True
                Dim url As dfinfo = dq.Dequeue()
                wc = New WebClientEx()
                wc.Headers.Add(HttpRequestHeader.Referer, "https://www.bilibili.com")
                wc.Headers.Add(HttpRequestHeader.Cookie, Cookie)
                wc.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88")
                wc.KeepAlive = True
                wc.Timeout = 10000000
                wc.text = url.path.Replace("/", "->")
                Dim d As New FileInfo(Application.StartupPath + "/" + url.path)
                d.Directory.Create()
                d.Create.Close()
                wc.fn = d.FullName
                wc.DownloadFileAsync(New Uri(url.url), d.FullName)
            Else
                Thread.Sleep(1000)
            End If

        End While
    End Sub

    Private Sub Webclient_DownloadFileCompleted(ByVal sender As Object, ByVal e As AsyncCompletedEventArgs) Handles wc.DownloadFileCompleted
        If e.Cancelled Then
            ThreadPool.QueueUserWorkItem(AddressOf AMsgBoxs, CType(sender, WebClientEx).text & vbCrLf & "下载被取消!")
        Else
            Dim s() As String = TryCast(sender, WebClientEx).fn.Split("\")
            If InStr(s(s.Length - 1), ".xml", CompareMethod.Binary) Then
                Dim ms As New MemoryStream(IO.File.ReadAllBytes(TryCast(sender, WebClientEx).fn))
                Dim dStream As New DeflateStream(ms, CompressionMode.Decompress)
                dStream.Flush()
                Dim sR As New StreamReader(dStream, Encoding.UTF8)
                IO.File.WriteAllText(TryCast(sender, WebClientEx).fn, sR.ReadToEnd())
            End If
            ThreadPool.QueueUserWorkItem(AddressOf AMsgBoxs, CType(sender, WebClientEx).text & vbCrLf & "下载完成!")
        End If
        lock = False
    End Sub

    Private Sub AMsgBoxs(state As Object)
        MsgBox(state.ToString)
    End Sub

    Private Sub Webclient_DownloadProgressChanged(ByVal sender As Object, ByVal e As DownloadProgressChangedEventArgs) Handles wc.DownloadProgressChanged
        If e.TotalBytesToReceive > 0 Then

            Dim dg As New setState(AddressOf setstat)
            Me.Invoke(dg, dq.Count.ToString, CType(sender, WebClientEx).text & vbCrLf & "->已下载: " & FormatBytes(e.BytesReceived) & vbCrLf & "->总大小: " & FormatBytes(e.TotalBytesToReceive) & vbCrLf & "->下载进度: " & Math.Round(((e.BytesReceived / e.TotalBytesToReceive) * 100), 2) & "%", e.ProgressPercentage)
        Else
            Dim dg As New setState(AddressOf setstat)
            Me.Invoke(dg, dq.Count.ToString, CType(sender, WebClientEx).text & vbCrLf & "->已下载: " & FormatBytes(e.BytesReceived), 100)

        End If

    End Sub

    Private Sub setstat(q As String, t As String, p As Integer)
        Label6.Text = "下载进度:  下载队列中有" + q + "个文件"
        Label5.Text = t
        ProgressBar1.Value = p
    End Sub

    Private Sub CheckForAllNodes(nodes As TreeNodeCollection)
        For i = 0 To nodes.Count - 1
            If nodes(i).Nodes.Count = 0 Then
                If nodes(i).Checked Then
                    Try
                        Dim di As dfinfo
                        di.url = nodes(i).Tag
                        Dim k() As String = nodes(i).Tag.ToString.Split("?")(0).Split("/")
                        di.path = nodes(i).Parent.Parent.Text + "/" + nodes(i).Parent.Text + "/" + k(k.Length - 1)
                        dq.Enqueue(di)
                    Catch ex As Exception

                    End Try
                End If
            Else
                CheckForAllNodes(nodes(i).Nodes)
            End If
        Next
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Try
            Dim client As New WebClientEx
            client.Timeout = 5000
            client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88")
            client.Encoding = Encoding.UTF8
            Dim loginauthkey As String = client.DownloadString("https://passport.bilibili.com/qrcode/getLoginUrl")
            Dim jo As JObject = JObject.Parse(loginauthkey)
            oauthKey = jo.SelectToken("data").SelectToken("oauthKey").ToString
            Dim qrcodeurl As String = jo.SelectToken("data").SelectToken("url").ToString
            Dim qrGenerator As QRCodeGenerator = New QRCodeGenerator()
            Dim playload As PayloadGenerator.Url = New PayloadGenerator.Url(qrcodeurl)
            Dim qrdata As QRCodeData = qrGenerator.CreateQrCode(playload)
            Dim Myqrcode As QRCode = New QRCode(qrdata)
            Dim qrCodeImage As Bitmap = Myqrcode.GetGraphic(5)
            PictureBox1.Image = qrCodeImage
        Catch ex As Exception
            MsgBox("获取失败：" & ex.Message)
        End Try
        GC.Collect()
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Try
            Dim client As New WebClientEx
            client.Timeout = 5000
            client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88")
            client.Encoding = Encoding.UTF8

            Dim authdat As String = client.UploadString("https://passport.bilibili.com/qrcode/getLoginInfo?oauthKey=" & oauthKey, "")

            Dim jo As JObject = JObject.Parse(authdat)
            Dim stat As String = jo.SelectToken("status").ToString
            If stat.ToLower = "true" Then
                Dim cook As String = jo.SelectToken("data").SelectToken("url").ToString
                Cookie = cook.Replace("https://passport.biligame.com/crossDomain?", "").Replace("&gourl=http%3A%2F%2Fwww.bilibili.com", "").Replace("&", "; ")
                PictureBox1.Image = defaultqrcode
                MsgBox("登录成功" & vbCrLf & client.ResponseHeaders.ToString & vbCrLf & cook)
            Else
                MsgBox("登录状态核验失败：未扫码或已失效，请重试！" & vbCrLf & authdat)
            End If
        Catch ex As Exception
            MsgBox("登录状态核验失败：" & ex.Message)
        End Try
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Dim c As String = InputBox("请输入Cookie", "手动Cookie输入")
        If c.Trim <> "" Then
            Cookie = c
        End If
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        Dim fn As String = Application.StartupPath & "/COOKIE_" & Math.Abs(Now.ToBinary) & ".txt"
        File.WriteAllText(fn, Cookie)
        MsgBox("保存成功:" & vbCrLf & fn)
    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        Try
            wc.CancelAsync()
        Catch ex As Exception

        End Try
    End Sub

    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        dq.Clear()
    End Sub
End Class
