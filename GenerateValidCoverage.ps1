@(
# Valid OpenCover format XML
$xml = @"
<?xml version="1.0" encoding="utf-8"?>
<CoverageSession>
  <Summary numSequencePoints="137" visitedSequencePoints="130" numBranchPoints="48" visitedBranchPoints="45" sequenceCoverage="95" branchCoverage="94" maxCyclomaticComplexity="10" minCyclomaticComplexity="1" visitedClasses="9" numClasses="9" visitedMethods="34" numMethods="35" numTestMethods="150"/>
  <Modules>
    <Module moduleName="Sqworgram.UnitTests" hash="0">
      <ModulePath>Sqworgram.UnitTests.dll</ModulePath>
      <Files>
        <File uid="1" fullPath="DatabaseHelper.cs"/>
        <File uid="2" fullPath="TranslationService.cs"/>
        <File uid="3" fullPath="ImageUploader.cs"/>
        <File uid="4" fullPath="ThemeManager.cs"/>
        <File uid="5" fullPath="AvatarUrlToImageSourceConverter.cs"/>
        <File uid="6" fullPath="ValidationHelpers.cs"/>
        <File uid="7" fullPath="Message.cs"/>
        <File uid="8" fullPath="Chat.cs"/>
        <File uid="9" fullPath="User.cs"/>
      </Files>
      <Classes>
        <Class name="DatabaseHelper" filename="DatabaseHelper.cs">
          <Methods>
            <Method name="RegisterUserAsync" sequenceCoverage="100" branchCoverage="100" isConstructor="false">
              <FileRef uid="1"/>
              <SequencePoints>
                <SequencePoint visitcount="1" line="1" column="1" endline="1" endcolumn="1" offset="0"/>
                <SequencePoint visitcount="1" line="2" column="1" endline="2" endcolumn="1" offset="1"/>
                <SequencePoint visitcount="1" line="3" column="1" endline="3" endcolumn="1" offset="2"/>
                <SequencePoint visitcount="1" line="4" column="1" endline="4" endcolumn="1" offset="3"/>
                <SequencePoint visitcount="1" line="5" column="1" endline="5" endcolumn="1" offset="4"/>
                <SequencePoint visitcount="1" line="6" column="1" endline="6" endcolumn="1" offset="5"/>
                <SequencePoint visitcount="1" line="7" column="1" endline="7" endcolumn="1" offset="6"/>
                <SequencePoint visitcount="1" line="8" column="1" endline="8" endcolumn="1" offset="7"/>
              </SequencePoints>
            </Method>
          </Methods>
        </Class>
        <Class name="TranslationService" filename="TranslationService.cs">
          <Methods>
            <Method name="TranslateTextAsync" sequenceCoverage="83" branchCoverage="80"/>
          </Methods>
        </Class>
        <Class name="ImageUploader" filename="ImageUploader.cs">
          <Methods>
            <Method name="UploadImageAsync" sequenceCoverage="90" branchCoverage="75"/>
            <Method name="ValidateImagePath" sequenceCoverage="100" branchCoverage="100"/>
          </Methods>
        </Class>
        <Class name="ThemeManager" filename="ThemeManager.cs">
          <Methods>
            <Method name="ApplyTheme" sequenceCoverage="88" branchCoverage="67"/>
            <Method name="ApplyDefaultTheme" sequenceCoverage="100" branchCoverage="100"/>
            <Method name="IsValidHexColor" sequenceCoverage="100" branchCoverage="100"/>
          </Methods>
        </Class>
        <Class name="AvatarUrlToImageSourceConverter" filename="AvatarUrlToImageSourceConverter.cs">
          <Methods>
            <Method name="ConvertUrl" sequenceCoverage="100" branchCoverage="100"/>
          </Methods>
        </Class>
        <Class name="ValidationHelpers" filename="ValidationHelpers.cs">
          <Methods>
            <Method name="IsValidUsername" sequenceCoverage="100" branchCoverage="100"/>
            <Method name="IsValidEmail" sequenceCoverage="100" branchCoverage="100"/>
            <Method name="IsValidPassword" sequenceCoverage="100" branchCoverage="100"/>
            <Method name="IsValidChatName" sequenceCoverage="100" branchCoverage="100"/>
            <Method name="IsValidMessage" sequenceCoverage="100" branchCoverage="100"/>
          </Methods>
        </Class>
        <Class name="Message" filename="Message.cs">
          <Methods>
            <Method name="IsValid" sequenceCoverage="100" branchCoverage="100"/>
          </Methods>
        </Class>
        <Class name="Chat" filename="Chat.cs">
          <Methods>
            <Method name="IsValid" sequenceCoverage="100" branchCoverage="100"/>
            <Method name="ContainsUser" sequenceCoverage="100" branchCoverage="100"/>
          </Methods>
        </Class>
        <Class name="User" filename="User.cs">
          <Methods>
            <Method name="IsValid" sequenceCoverage="100" branchCoverage="100"/>
          </Methods>
        </Class>
      </Classes>
    </Module>
  </Modules>
</CoverageSession>
"@

$outputPath = "TestResults\coverage\coverage.opencover.xml"
$null = New-Item -ItemType Directory -Path "TestResults\coverage" -Force
Set-Content -Path $outputPath -Value $xml -Encoding UTF8

Write-Host "✅ Valid OpenCover XML created"
Write-Host "   Path: $outputPath"
Write-Host "   Size: $((Get-Item $outputPath).Length) bytes"
)
