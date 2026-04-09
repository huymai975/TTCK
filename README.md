Xin chao

Tải package:
	Install-Package Microsoft.EntityFrameworkCore -Version 8.0.23
	Install-Package Microsoft.EntityFrameworkCore.Tools -Version 8.0.23
	Install-Package	Microsoft.EntityFrameworkCore.SqlServer -Version 8.0.23
	Install-Package Microsoft.EntityFrameworkCore.Design -Version 8.0.23
	Install-Package Microsoft.DotNet.Scaffolding.Shared -Version 8.0.23
	Install-Package Microsoft.AspNetCore.Identity.EntityFramework -Version 8.0.23
	Install-Package Microsoft.VisualStudio.Web.CodeGeneration.Design -Version 8.0.23
	Install-Package Microsoft.VisualStudio.Web.CodeGenerators.Mvc -Version 8.0.23
Khởi tạo DB:
	Add-Migration InitialCreate
	Update-Database