# ������ ������� 

� ������ ������� � ����� appsettings.json �������� � ������ {"ConnectionString": "Server=localhost;Port=5432;Database=ApplicationsDataBase;User id=postgres;Password=1928"} �������� Password �� �������� ������ �� pgAdmin ������ ����������
#### Password=�����_�����������_������_��_postgres

������� ����� ������� (����� � ������� �������� CollectingApplicationsAPI.csproj) � ������� �� �� PowerShell
����� ���� ��������� ��� �������

dotnet ef migrations add Initial -c ApplicationContext --project ..\CollectingApplicationsAPI
dotnet ef database update Initial -c ApplicationContext --project ..\CollectingApplicationsAPI

### ������ ������� ������� ��������, ������ �� ������ ���� �������� ������� ����


## ���� ��������� �� ���������, �� ������ ����� � �������� �� �����������������