[Setup]
AppName=OM CartridgeDesk
AppVersion=1.0
DefaultDirName={pf}\OM CartridgeDesk
DefaultGroupName=OM CartridgeDesk
UninstallDisplayIcon={app}\OM_CartridgeDesk.exe
Compression=lzma2
SolidCompression=yes

[Files]
Source: "C:\Users\Lutikov_DA\source\repos\OM_CartridgeDesk\OM_CartridgeDesk\bin\Release\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Code]
var
  DatabasePath: string;

function GetDatabasePathParam: string;
var
  i: Integer;
  Param: string;
begin
  Result := '';
  for i := 1 to ParamCount do
  begin
    Param := ParamStr(i);
    if Pos('/DBPATH=', Param) = 1 then
    begin
      Result := Copy(Param, 9, Length(Param) - 8);
      while Pos('"', Result) > 0 do
        Delete(Result, Pos('"', Result), 1);
      Break;
    end;
  end;
end;

function InitializeSetup: Boolean;
begin
  DatabasePath := GetDatabasePathParam;
  if DatabasePath = '' then
  begin
    MsgBox('Не передан обязательный параметр /DBPATH="путь_к_БД"', mbError, MB_OK);
    Result := False;
    Exit;
  end;
  Result := True;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  ConfigPath: string;
  ConfigContent: string;
  SearchStr: string;
  NewValue: string;
  P: Integer;
begin
  if CurStep = ssPostInstall then
  begin
    ConfigPath := ExpandConstant('{app}\OM_CartridgeDesk.exe.config');
    if FileExists(ConfigPath) then
    begin
      if LoadStringFromFile(ConfigPath, ConfigContent) then
      begin
        SearchStr := 'value="CartridgeDesk.accdb"';
        NewValue := 'value="' + DatabasePath + '"';
        P := Pos(SearchStr, ConfigContent);
        if P > 0 then
        begin
          Delete(ConfigContent, P, Length(SearchStr));
          Insert(NewValue, ConfigContent, P);
          SaveStringToFile(ConfigPath, ConfigContent, False);
        end;
      end;
    end;
  end;
end;