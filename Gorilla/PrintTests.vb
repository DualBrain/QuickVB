Module PrintTests

  Sub Main()

    Print("Hello World!")
    Print("1+1 =", 1 + 1)
    Print("a", qbSemicolon) : Print("b", qbComma) : Print("c")
    Print(".")
    Print("a", qbComma, "b", qbComma, "c", qbComma, "d", qbComma, "e", qbComma, "f", qbComma, "g", qbComma, "h", qbComma, "i")
    Print(".")
    Print(".") : Locate(8, 40) : Print(QBString(60, "="))
    Print(".")
    Print("Done.")

    Sleep()

  End Sub

End Module
