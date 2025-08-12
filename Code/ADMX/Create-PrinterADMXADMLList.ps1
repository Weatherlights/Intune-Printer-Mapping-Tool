param(
    [string]$FilePath = ".\letter"
)

$letters = @(0..29);
$contexts = @("User", "Machine");

rm "$($FilePath)_admx.xml";
rm "$($FilePath)_string.xml";
rm "$($FilePath)_presentation.xml";


forEach ( $context in $contexts) {
    foreach ( $letter in $letters ) {

   $xml = "<policy name=`"Printer_$($letter)_$($context)`" class=`"$($context)`" displayName=`"`$(string.Printer_$($letter)_$($context)_DisplayName)`" explainText=`"`$(string.Printer_$($letter)_$($context)_ExplainText)`" presentation=`"`$(presentation.Printer_$($letter)_$($context)_Presentation)`" key=`"Software\Policies\weatherlights.com\NetworkDriveMapping\Policies`" valueName=`"$($letter)`">
      <parentCategory ref=`"Printers`" />
      <supportedOn ref=`"SupportedOn`" />
      <elements>
        <text id=`"Printer_$($letter)_$($context)_Path`" key=`"Software\Policies\weatherlights.com\NetworkDriveMapping\Policies\$($letter)`" valueName=`"Path`" required=`"true`" />
        <enum id=`"Printer_$($letter)_$($context)_Operation`" valueName=`"Operation`" required=`"true`">
            <item displayName=`"`$(string.Add)`">
                <value>
                    <string>Add</string>
                </value>
            </item>
            <item displayName=`"`$(string.Delete)`">
                <value>
                    <string>Delete</string>
                </value>
            </item>
        </enum>
        <boolean id=`"Printer_$($letter)_$($context)_SetDefault`" valueName=`"SetDefault`" />
      </elements>
    </policy>" 

    $string="<string id=`"Printer_$($letter)_$($context)_DisplayName`">Printer operation $($letter)</string>
    <string id=`"Printer_$($letter)_$($context)_ExplainText`">Printer operation $($letter). Specify the printer path and the operation (Add or Remove).</string>

";

    $presentation="<presentation id=`"Printer_$($letter)_$($context)_Presentation`">
    <textBox refId=`"Printer_$($letter)_$($context)_Path`"><label>Path</label><defaultValue></defaultValue></textBox>
    <dropdownList refId=`"Printer_$($letter)_$($context)_Operation`">Operation</dropdownList>
    <checkBox refId=`"Printer_$($letter)_$($context)_SetDefault`">Set Default</checkBox>
</presentation>

"


    $xml | Out-File -FilePath "$($FilePath)_admx.xml" -Append
    $string | Out-File -FilePath "$($FilePath)_string.xml" -Append
    $presentation | Out-File -FilePath "$($FilePath)_presentation.xml" -Append
}
}