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

Public Class Vpartsinfo

    Public partsname As String
    Public cid As String
    Public quality As String
    Public urls As New List(Of String)
    Public Additional As New List(Of String)

    Public Sub setQuality(s As String)
        quality = s
    End Sub

    Public Function getQuality() As String
        Select Case quality
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

End Class
