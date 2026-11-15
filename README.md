# SharedCost Portal – Hệ thống Quản lý và Phân bổ Chi phí Dùng chung

**SharedCost Portal** là hệ thống quản trị tài chính doanh nghiệp hỗ trợ khai báo, phân loại, mô phỏng, phân bổ chính thức, kiểm duyệt, khóa sổ và truy xuất nguồn gốc các khoản chi phí dùng chung theo từng kỳ kế toán YYYY-MM


1. Tính năng Nghiệp vụ Chính

- Quản lý Dữ liệu Đầu vào:
  - Khoản chi phí dùng chung Cost Pool: Khai báo nguồn chi phí theo kỳ kế toán, số tiền nguồn, chứng từ tham chiếu và trạng thái áp dụng.
  - Tiêu thức phân bổ Driver & Driver Value : Quản lý danh mục tiêu thức (diện tích sử dụng, số lượng nhân sự, giờ vận hành...) và giá trị đo lường thực tế theo từng đơn vị nhận và từng kỳ.
  - Đối tượng nhận chi phí Target: Quản lý danh sách phòng ban, trung tâm chi phí hoặc đơn vị kinh doanh thụ hưởng.
- Quản lý Quy tắc & Phiên bản Allocation Rule & Rule Version
  - Thiết lập quy tắc liên kết khoản chi phí nguồn, tiêu thức phân bổ, biểu thức tính và danh sách đơn vị nhận mặc định.
  - Quản lý lịch sử phiên bản bất biến Rule Version: mỗi khi thay đổi cấu hình, hệ thống phát hành phiên bản mới và đóng băng danh sách đơn vị nhận RuleVersionTarget để bảo toàn dữ liệu lịch sử.
- Mô phỏng & Phân bổ Chính thức Simulation & Allocation Run
  - Mô phỏng : Tính thử tỷ lệ và số tiền phân bổ dự kiến để đánh giá phương án mà không làm thay đổi số liệu chính thức.
  - Phân bổ chính thức : Thực thi tính toán trong giao dịch cơ sở dữ liệu, áp dụng thuật toán làm tròn phần dư lớn nhất  ở 2 chữ số thập phân, đảm bảo tổng tỷ lệ bằng 1.000000 và tổng tiền phân bổ khớp 100% tổng tiền nguồn.
- Kiểm soát Vòng đời & Khóa sổ :
  - Kiểm soát chặt chẽ vòng đời trạng thái: DRAFT → CALCULATED → PENDING_REVIEW → REVIEWED (hoặc REJECTED kèm lý do bắt buộc) → LOCKED.
  - Dữ liệu sau khi khóa sổ được bảo vệ bất biến tại cả tầng nghiệp vụ lẫn SQL Server Triggers.
- Truy xuất Nguồn gốc, Kiểm toán & Báo cáo :
  - Truy ngược từ từng dòng kết quả phân bổ về phiên chạy, phiên bản quy tắc, khoản chi phí nguồn, bản ghi tiêu thức gốc và chứng từ tham chiếu.
  - Ghi nhận nhật ký thao tác hệ thống kèm dữ liệu JSON trước và sau khi thay đổi.
  - Bảng điều khiển tổng hợp 6 chỉ số KPI, biểu đồ phân bổ theo đơn vị nhận, biểu đồ theo kỳ kế toán và cảnh báo nghiệp vụ.


## Kiến trúc và Công nghệ Sử dụng

Hệ thống được xây dựng theo kiến trúc phân tầng Clean Architecture:

- Backend: C#, ASP.NET Core Web API .NET 10, Entity Framework Core.
- Cơ sở dữ liệu: Microsoft SQL Server gồm 17 bảng, các khóa chính/khóa ngoại tổ hợp, ràng buộc toàn vẹn và 4 AFTER TRIGGERS.
- Xác thực & Phân quyền: OpenID Connect OIDC Bearer Token kết hợp phân quyền theo vai trò RBAC: ADMIN, FINANCE, REVIEWER AUDITOR, VIEWER.
- Frontend: React 18, TypeScript, Vite, Axios, Lucide Icons.
- Kiểm thử tự động: xUnit SharedCost.UnitTests.
- Triển khai: Docker & Docker Compose.



## Yêu cầu Môi trường Cài đặt

- .NET SDK: Phiên bản .NET 10.0 trở lên.
- Node.js: Phiên bản 18.x hoặc 20.x trở lên (đi kèm npm).
- Cơ sở dữ liệu: Microsoft SQL Server (Express / Developer Edition) và SQL Server Management Studio (SSMS).
- IDE (tùy chọn): Visual Studio 2022/2026 hoặc Visual Studio Code.

##  Hướng dẫn Cài đặt và Chạy Chương trình

### Bước 1: Khởi tạo Cơ sở dữ liệu SQL Server
1. Mở SQL Server Management Studio (SSMS) và kết nối vào máy chủ SQL Server trên máy
2. SQL trong thư mục database/ và nhấn Execute  để khởi tạo cơ sở dữ liệu PhanBoChiPhi cùng toàn bộ bảng, Trigger và dữ liệu mẫu.
3. Kiểm tra chuỗi kết nối DefaultConnection trong tệp src/SharedCost.API/appsettings.json sao cho khớp với tên Server SQL trên máy của bạn:

   "ConnectionStrings": {
     "DefaultConnection": "Server=.\\SQLEXPRESS;Database=PhanBoChiPhi;Trusted_Connection=True;TrustServerCertificate=True;"
   }

### Bước 2: Khởi chạy Backend API 
Mở cửa sổ dòng lệnh (Terminal / PowerShell) tại thư mục gốc của dự án và chạy:


dotnet restore
dotnet build src/SharedCost.API/SharedCost.API.csproj
dotnet run --project src/SharedCost.API/SharedCost.API.csproj --urls "http://localhost:5080"


- Tài liệu API tương tác (Swagger UI): http://localhost:5080/swagger
- Kiểm tra trạng thái máy chủ (Health Check): http://localhost:5080/health

### Bước 3: Khởi chạy Giao diện Frontend 
Mở một cửa sổ dòng lệnh thứ hai (giữ nguyên cửa sổ Backend ở Bước 2 đang chạy) và thực hiện:

cd frontend
npm install
npm run dev

- Mở trình duyệt web truy cập vào địa chỉ: http://localhost:5173


##  Tài khoản Kiểm thử theo Vai trò (Development Demo)

Tại màn hình đăng nhập http://localhost:5173, có thể nhập trực tiếp tên đăng nhập / email công vụ hoặc sử dụng tính năng Chế độ Demo  với 5 tài khoản mẫu đã nạp sẵn trong cơ sở dữ liệu:

admin  admin@sharedcost.local Toàn quyền quản trị người dùng, phân quyền và danh mục hệ thống 
finance  finance@sharedcost.local  Khai báo dữ liệu đầu vào, thiết lập quy tắc, chạy mô phỏng và phân bổ 
reviewer  reviewer@sharedcost.local  Kiểm tra, phê duyệt hoặc từ chối kết quả và thực hiện khóa sổ 
auditor auditor@sharedcost.local  Tra cứu nguồn gốc dữ liệu, xem nhật ký kiểm toán và báo cáo
viewe viewer@sharedcost.local  Xem dữ liệu tổng quan và các báo cáo phân bổ được cấp quyền 


##  Chạy Kiểm thử Tự động 

Để chạy bộ kiểm thử đơn vị kiểm tra công thức phân bổ, thuật toán làm tròn phần dư và các ràng buộc nghiệp vụ:


dotnet test tests/SharedCost.UnitTests/SharedCost.UnitTests.csproj --logger "console;verbosity=normal"


## Triển khai bằng Docker Compose 

Nếu máy tính đã cài đặt **Docker Desktop**, bạn có thể khởi chạy đồng thời SQL Server, Backend API và Frontend chỉ với một lệnh:


Copy-Item .env.example .env
# Cập nhật SQL_SA_PASSWORD và OIDC_SIGNING_SECRET trong tệp .env trước khi chạy
docker compose up -d --build

