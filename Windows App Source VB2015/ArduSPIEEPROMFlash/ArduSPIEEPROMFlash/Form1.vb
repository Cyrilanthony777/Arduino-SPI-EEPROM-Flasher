Imports System
Imports System.IO
Imports System.Text
Public Class Form1

    Dim array(50) As String
    Dim mySerial As New IO.Ports.SerialPort
    Dim outgoing As String = ""
    Dim savloc As String = ""
    Dim opnloc As String = ""
    Dim cmdread As Byte() = {&HFF, &HEE, &HFF, &HEE, &H1, &H1, &H1, &H1, &HAA}
    Dim cmdwrite As Byte() = {&HFF, &HEE, &HFF, &HEE, &H1, &H1, &H1, &H1, &HCC}
    Dim cmderase As Byte() = {&HFF, &HEE, &HFF, &HEE, &H1, &H1, &H1, &H1, &HEE}
    Dim cmddcrc As Byte() = {&HFF, &HEE, &HFF, &HEE, &H1, &H1, &H1, &H1, &HDC}
    Dim CMD_ERASE(40) As Byte
    Dim CMD_READ(40) As Byte
    Dim CMD_WRITE(40) As Byte
    Dim CMD_DCRC(40) As Byte
    Dim erzcnt As Integer = 0
    Dim erzdel As Integer = 35


    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        For Each sp As String In My.Computer.Ports.SerialPortNames
            ComboBox1.Items.Add(sp)
        Next

        Button2.Enabled = False
        Button5.Enabled = False
        Button6.Enabled = False
        Button7.Enabled = False
        For x As Integer = 0 To cmdread.Length - 1
            CMD_READ(x) = cmdread(x)
        Next

        For x As Integer = 0 To cmdwrite.Length - 1
            CMD_WRITE(x) = cmdwrite(x)
        Next

        For x As Integer = 0 To cmderase.Length - 1
            CMD_ERASE(x) = cmderase(x)
        Next

        For x As Integer = 0 To cmddcrc.Length - 1
            CMD_DCRC(x) = cmddcrc(x)
        Next

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If mySerial.IsOpen() = False Then

            Try

                mySerial = My.Computer.Ports.OpenSerialPort(ComboBox1.Text)
                mySerial.BaudRate = 115200
                mySerial.ReadTimeout = 3000


            Catch ex As Exception
                MsgBox("Error Opening COM port")
            End Try
            If mySerial.IsOpen = True Then
                Label4.Text = "Connected"
                Button1.Enabled = False
                Button2.Enabled = True
                Button5.Enabled = True
                Button6.Enabled = True
                Button7.Enabled = True

            End If
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If mySerial.IsOpen = True Then
            mySerial.Close()
            Button1.Enabled = True
            Button2.Enabled = False
            Button5.Enabled = False
            Button6.Enabled = False
            Button7.Enabled = False
            Label4.Text = "Disonnected"

        End If
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        SaveFileDialog1.Filter = "Binary File|*.bin"
        SaveFileDialog1.Title = "Save EEPROM dump"
        If SaveFileDialog1.ShowDialog = DialogResult.OK Then

            TextBox1.Text = SaveFileDialog1.FileName
            savloc = TextBox1.Text

        End If


    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Dim ads As Integer = Val(TextBox3.Text)
        Dim lengths() As Byte = BitConverter.GetBytes(ads)
        CMD_READ(4) = lengths(3)
        CMD_READ(5) = lengths(2)
        CMD_READ(6) = lengths(1)
        CMD_READ(7) = lengths(0)

        If mySerial.IsOpen = True Then
            If savloc = "" Then

                MsgBox("Please Select a location to Save the file")
            Else
                Dim fs As FileStream = File.Create(savloc)
                Dim value As Integer = 0
                Dim incoming As Byte
                Button2.Enabled = False
                Button5.Enabled = False
                Button6.Enabled = False
                Button7.Enabled = False
                ProgressBar1.Minimum = value
                ProgressBar1.Maximum = ads
                mySerial.Write(CMD_READ, 0, 41)

                For value = 0 To ads
                    Try
                        incoming = mySerial.ReadByte()
                        fs.WriteByte(incoming)
                        Label4.Text = "Reading"
                    Catch ex As TimeoutException
                        MsgBox("Read Timeout")
                        fs.Close()
                        Button2.Enabled = True
                        Button5.Enabled = True
                        Button6.Enabled = True
                        Button7.Enabled = True
                        Exit For
                    End Try
                    ProgressBar1.Value = value

                Next
                Button2.Enabled = True
                Button5.Enabled = True
                Button6.Enabled = True
                Button7.Enabled = True
                If value >= ads Then
                    MsgBox("Read Completed")
                    Label4.Text = "Read Completed"

                End If
                fs.Close()
            End If

        End If

    End Sub

    Private Sub TextBox3_TextChanged(sender As Object, e As EventArgs) Handles TextBox3.TextChanged

    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click

        If mySerial.IsOpen = True Then
            ProgressBar1.Minimum = 0
            ProgressBar1.Maximum = erzdel

            mySerial.Write(CMD_ERASE, 0, 41)
            Timer1.Start()


        End If
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick



        If erzcnt <> erzdel Then
            erzcnt = erzcnt + 1
            ProgressBar1.Value = erzcnt
            Button2.Enabled = False
            Button5.Enabled = False
            Button6.Enabled = False
            Button7.Enabled = False
            Label4.Text = "Erasing Chip"

        Else
            erzcnt = 0
            MsgBox("Chip Erased")
            Button2.Enabled = True
            Button5.Enabled = True
            Button6.Enabled = True
            Button7.Enabled = True
            Label4.Text = "Chip Erased"
            Timer1.Stop()
            MsgBox("Erase Completed")
        End If
    End Sub

    Private Sub Button9_Click(sender As Object, e As EventArgs)
        Dim wrdta As Byte()
        wrdta = {&HFF, &HEE, &HFF, &HEE, &H0, &H0, &H0, &H0, &HDD, &H0}
        If mySerial.IsOpen() Then
            mySerial.Write(wrdta, 0, 10)
            Try
                mySerial.ReadLine()
            Catch ex As TimeoutException
                Label6.Text = "TOT"
            End Try
        End If

    End Sub

    Private Sub Button8_Click(sender As Object, e As EventArgs)
        Dim opfs As FileStream = File.OpenRead(opnloc)
        Dim fdata As Byte() = File.ReadAllBytes(opnloc)
        Dim flen As Long = fdata.GetLength(0)
        Dim maxpg As Integer = flen / 256
        Label6.Text = "Writing Device"
        Dim incd As Byte
        Dim addr As Integer = 0
        Dim curpg As Integer = 0
        Dim curpgasbyte As Byte()
        Dim crc(8) As Byte
        Dim Dcrc As Byte
        Dim subpgLoc(8) As Integer

        Dim page As Integer = 0
        Dim wrdta As Byte()
        Dim subpg As Byte
        Dim wrbuff(40, 8) As Byte
        Dim spbuff1(40) As Byte
        Dim spbuff2(40) As Byte
        Dim spbuff3(40) As Byte
        Dim spbuff4(40) As Byte
        Dim spbuff5(40) As Byte
        Dim spbuff6(40) As Byte
        Dim spbuff7(40) As Byte
        Dim spbuff8(40) As Byte



        ProgressBar1.Minimum = 0
        ProgressBar1.Maximum = maxpg

        For curpg = 0 To 1
            addr = curpg * 256
            subpgLoc(0) = addr
            For x As Integer = 1 To 8
                subpgLoc(x) = addr + (x * 32)
            Next
            curpgasbyte = BitConverter.GetBytes(curpg)
            CMD_WRITE(4) = curpgasbyte(1)
            CMD_WRITE(5) = curpgasbyte(0)
            For subpg = 1 To 8
                wrdta = {&HFF, &HEE, &HFF, &HEE, curpgasbyte(1), curpgasbyte(0), subpg, &H1, &H7A}
                For xo As Integer = 0 To wrdta.Length - 1
                    If subpg = 1 Then
                        spbuff1(xo) = wrdta(xo)
                    ElseIf subpg = 2 Then
                        spbuff2(xo) = wrdta(xo)
                    ElseIf subpg = 3 Then
                        spbuff3(xo) = wrdta(xo)
                    ElseIf subpg = 4 Then
                        spbuff4(xo) = wrdta(xo)
                    ElseIf subpg = 5 Then
                        spbuff5(xo) = wrdta(xo)
                    ElseIf subpg = 6 Then
                        spbuff6(xo) = wrdta(xo)
                    ElseIf subpg = 7 Then
                        spbuff7(xo) = wrdta(xo)
                    ElseIf subpg = 8 Then
                        spbuff8(xo) = wrdta(xo)
                    End If
                Next
                Dim t As Integer = 8
                For g As Integer = subpgLoc(subpg - 1) To subpgLoc(subpg) - 1
                    t = t + 1
                    If subpg = 1 Then
                        spbuff1(t) = fdata(g)
                    ElseIf subpg = 2 Then
                        spbuff2(t) = fdata(g)
                    ElseIf subpg = 3 Then
                        spbuff3(t) = fdata(g)
                    ElseIf subpg = 4 Then
                        spbuff4(t) = fdata(g)
                    ElseIf subpg = 5 Then
                        spbuff5(t) = fdata(g)
                    ElseIf subpg = 6 Then
                        spbuff6(t) = fdata(g)
                    ElseIf subpg = 7 Then
                        spbuff7(t) = fdata(g)
                    ElseIf subpg = 8 Then
                        spbuff8(t) = fdata(g)
                    End If
                    crc(subpg) = crc(subpg) Xor fdata(g)
                Next
                Dcrc = Dcrc Xor crc(subpg)

            Next


            mySerial.Write(spbuff1, 0, 41)
            mySerial.Write(spbuff2, 0, 41)
            mySerial.Write(spbuff3, 0, 41)
            mySerial.Write(spbuff4, 0, 41)
            mySerial.Write(spbuff5, 0, 41)
            mySerial.Write(spbuff6, 0, 41)
            mySerial.Write(spbuff7, 0, 41)
            mySerial.Write(spbuff8, 0, 41)



        Next



    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        OpenFileDialog1.Filter = "Binary File|*.bin"
        OpenFileDialog1.Title = "Open EEPROM dump to Write"
        If OpenFileDialog1.ShowDialog() = DialogResult.OK Then
            TextBox2.Text = OpenFileDialog1.FileName
            opnloc = TextBox2.Text
        End If
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        Dim opfs As FileStream = File.OpenRead(opnloc)
        Dim fdata As Byte() = File.ReadAllBytes(opnloc)
        Dim flen As Long = fdata.GetLength(0)
        Dim maxpg As Integer = flen / 256
        Label6.Text = "Writing Device"
        Dim incd(256) As Byte
        Dim xs As Byte
        Dim addr As Integer = 0
        Dim curpg As Integer = 0
        Dim curpgasbyte As Byte()
        Dim crc(8) As Byte
        Dim Dcrc As Byte
        Dim subpgLoc(8) As Integer
        Dim recrc(8) As Byte
        Dim page As Integer = 0
        Dim wrdta As Byte()
        Dim subpg As Byte
        Dim wrbuff(40, 8) As Byte
        Dim spbuff1(40) As Byte
        Dim spbuff2(40) As Byte
        Dim spbuff3(40) As Byte
        Dim spbuff4(40) As Byte
        Dim spbuff5(40) As Byte
        Dim spbuff6(40) As Byte
        Dim spbuff7(40) As Byte
        Dim spbuff8(40) As Byte



        ProgressBar1.Minimum = 0
        ProgressBar1.Maximum = 1000

        For curpg = 0 To 1000
            addr = curpg * 256
            subpgLoc(0) = addr
            Dcrc = &H0
            ProgressBar1.Value = curpg
            For x As Integer = 1 To 8
                crc(x) = &H0
            Next
            For x As Integer = 1 To 8
                subpgLoc(x) = addr + (x * 32)
            Next
            curpgasbyte = BitConverter.GetBytes(curpg)
            CMD_WRITE(4) = curpgasbyte(1)
            CMD_WRITE(5) = curpgasbyte(0)
            For subpg = 1 To 8
                wrdta = {&HFF, &HEE, &HFF, &HEE, curpgasbyte(1), curpgasbyte(0), subpg, &H1, &H7A}
                For xo As Integer = 0 To wrdta.Length - 1
                    If subpg = 1 Then
                        spbuff1(xo) = wrdta(xo)
                    ElseIf subpg = 2 Then
                        spbuff2(xo) = wrdta(xo)
                    ElseIf subpg = 3 Then
                        spbuff3(xo) = wrdta(xo)
                    ElseIf subpg = 4 Then
                        spbuff4(xo) = wrdta(xo)
                    ElseIf subpg = 5 Then
                        spbuff5(xo) = wrdta(xo)
                    ElseIf subpg = 6 Then
                        spbuff6(xo) = wrdta(xo)
                    ElseIf subpg = 7 Then
                        spbuff7(xo) = wrdta(xo)
                    ElseIf subpg = 8 Then
                        spbuff8(xo) = wrdta(xo)
                    End If
                Next
                Dim t As Integer = 8
                For g As Integer = subpgLoc(subpg - 1) To subpgLoc(subpg) - 1
                    t = t + 1
                    If subpg = 1 Then
                        spbuff1(t) = fdata(g)
                    ElseIf subpg = 2 Then
                        spbuff2(t) = fdata(g)
                    ElseIf subpg = 3 Then
                        spbuff3(t) = fdata(g)
                    ElseIf subpg = 4 Then
                        spbuff4(t) = fdata(g)
                    ElseIf subpg = 5 Then
                        spbuff5(t) = fdata(g)
                    ElseIf subpg = 6 Then
                        spbuff6(t) = fdata(g)
                    ElseIf subpg = 7 Then
                        spbuff7(t) = fdata(g)
                    ElseIf subpg = 8 Then
                        spbuff8(t) = fdata(g)
                    End If
                    crc(subpg) = crc(subpg) Xor fdata(g)
                Next
                Dcrc = Dcrc Xor crc(subpg)

            Next
            Try


                mySerial.Write(spbuff1, 0, 41)
                recrc(1) = mySerial.ReadByte()
                If recrc(1) <> crc(1) Then
                    MsgBox("CRC Error!!")
                    Exit For
                End If
                mySerial.Write(spbuff2, 0, 41)
                recrc(2) = mySerial.ReadByte()
                If recrc(2) <> crc(2) Then
                    MsgBox("CRC Error!!")
                    Exit For
                End If
                mySerial.Write(spbuff3, 0, 41)
                recrc(3) = mySerial.ReadByte()
                If recrc(3) <> crc(3) Then
                    MsgBox("CRC Error!!")
                    Exit For
                End If
                mySerial.Write(spbuff4, 0, 41)
                recrc(4) = mySerial.ReadByte()
                If recrc(4) <> crc(4) Then
                    MsgBox("CRC Error!!")
                    Exit For
                End If
                mySerial.Write(spbuff5, 0, 41)
                recrc(5) = mySerial.ReadByte()
                If recrc(5) <> crc(5) Then
                    MsgBox("CRC Error!!")
                    Exit For
                End If
                mySerial.Write(spbuff6, 0, 41)
                recrc(6) = mySerial.ReadByte()
                If recrc(6) <> crc(6) Then
                    MsgBox("CRC Error!!")
                    Exit For
                End If
                mySerial.Write(spbuff7, 0, 41)
                recrc(7) = mySerial.ReadByte()
                If recrc(7) <> crc(7) Then
                    MsgBox("CRC Error!!")
                    Exit For
                End If
                mySerial.Write(spbuff8, 0, 41)
                recrc(8) = mySerial.ReadByte()
                If recrc(8) <> crc(8) Then
                    MsgBox("CRC Error!!")
                    Exit For
                End If

                mySerial.Write(CMD_DCRC, 0, 41)
                recrc(0) = mySerial.ReadByte()
                If recrc(0) <> Dcrc Then
                    MsgBox("DCRC Error!!. Expected: " + Dcrc.ToString() + " Returned: " + recrc(0).ToString())
                    Exit For
                End If

                mySerial.Write(CMD_WRITE, 0, 41)
                xs = mySerial.ReadByte()
                If xs <> &HD7 Then
                    MsgBox("Read Write Mismatch at page: " + curpg.ToString())
                    Exit For
                End If
                System.Threading.Thread.Sleep(10)

            Catch ex As TimeoutException
                MsgBox("Read Timeout")
                Exit For

            End Try

        Next
        MsgBox("Write Complete")
        Label6.Text = "Write Complete"

    End Sub
End Class
