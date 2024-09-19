Imports Microsoft.VisualBasic

Public Enum dbConnectionType
    ole = 0
    sqlclient = 1
    odbc = 3
End Enum

''' <summary>
''' WebType.iWebCore means that the page is core to all iWeb applications
''' Any pages with this set as there page attribute will be accessible by all iWeb variations
''' </summary>
''' <remarks></remarks>
Public Enum WebType
    iWebCore = 0
    iWebPayrollOnly = 1
    iWebUnlicensed = 2
End Enum

Public Enum urlType
    url = 0
    form = 1
    list = 2
End Enum

Public Enum IAWDerivationType
    none = 0
    plaintext = 1
    sql = 2
    derived = 3
End Enum

Public Enum IAWWebListColumnType
    text = 0
    [date] = 1
    time = 2
    pupText = 3
    lookup = 4
    checkbox = 5
    image = 6
    urlButton = 7
End Enum


Public Enum WebAttributeType
    text = 1
    checkbox = 2
    puptext = 3
    combination = 4
    number = 5
    sql = 6
    urlButton = 7
End Enum

Public Enum WebObjectType
    form = 1
    list = 2
    grid = 3
    formItem = 4
    listItem = 5
    gridItem = 6
    menu = 7
End Enum

