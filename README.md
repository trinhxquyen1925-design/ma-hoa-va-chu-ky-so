\# MaHoaVaChuKySo



Bộ công cụ minh họa các thuật toán mã hóa và bảo mật thông tin, được xây dựng bằng \*\*C# trên .NET 10\*\* cho bài tập môn \*\*An toàn và bảo mật thông tin\*\*. Dự án cung cấp cả giao diện dòng lệnh (Console) để kiểm thử và giao diện WinForms để thao tác trực quan.



\## Các chức năng chính



\- Mã hóa đối xứng AES.

\- Mã hóa bất đối xứng RSA với OAEP-SHA256.

\- Hàm băm SHA-256 và thử nghiệm hiệu ứng Avalanche.

\- Chữ ký số RSA-PSS và kiểm tra chữ ký.

\- Benchmark AES và RSA, xuất dữ liệu kết quả ra CSV.



!\[Giao diện chính](docs/screenshots/main-ui.png)



\## Mục lục



\- \[Kiến trúc dự án](#kiến-trúc-dự-án)

\- \[Yêu cầu](#yêu-cầu)

\- \[Cài đặt và chạy](#cài-đặt-và-chạy)

\- \[Các chức năng và kết quả](#các-chức-năng-và-kết-quả)

&#x20; - \[1. AES](#1-aes)

&#x20; - \[2. RSA và định dạng PEM](#2-rsa-và-định-dạng-pem)

&#x20; - \[3. SHA-256 và hiệu ứng Avalanche](#3-sha-256-và-hiệu-ứng-avalanche)

&#x20; - \[4. Chữ ký số](#4-chữ-ký-số)

&#x20; - \[5. Benchmark RSA](#5-benchmark-rsa)

\- \[Nhận xét từ kết quả đo](#nhận-xét-từ-kết-quả-đo)

\- \[Lưu ý bảo mật](#lưu-ý-bảo-mật)



\---



\## Kiến trúc dự án



Solution gồm 3 project, sử dụng chung một thư viện lõi:



| Project | Framework | Vai trò |

| --- | --- | --- |

| `MaHoaVaChuKySo.Core` | `net10.0` | Chứa các thuật toán và chức năng cốt lõi, không phụ thuộc giao diện. |

| `MaHoaVaChuKySo.Console` | `net10.0` | Chạy demo từng tính năng và in kết quả kiểm thử `PASS/FAIL`. |

| `MaHoaVaChuKySo.Desktop` | `net10.0-windows` | Giao diện WinForms gồm 5 tab: AES, RSA, Hash, Chữ ký và Benchmark. |



\## Yêu cầu



\- Cài đặt \*\*.NET 10 SDK\*\*.

\- Sử dụng Windows để chạy ứng dụng WinForms `MaHoaVaChuKySo.Desktop`.

\- Mở Terminal tại thư mục chứa file solution `MaHoaVaChuKySo.slnx`.



\## Cài đặt và chạy



\### 1. Build toàn bộ solution



```powershell

dotnet build MaHoaVaChuKySo.slnx -c Release

```



\### 2. Chạy chương trình Console



```powershell

dotnet run -c Release --project src\\MaHoaVaChuKySo.Console

```



\### 3. Chạy giao diện WinForms



```powershell

dotnet run -c Release --project src\\MaHoaVaChuKySo.Desktop

```



\### 4. Chạy benchmark AES



Chạy kiểm thử nhanh:



```powershell

dotnet run -c Release --project src\\MaHoaVaChuKySo.Console -- benchmark quick

```



Chạy kiểm thử đầy đủ:



```powershell

dotnet run -c Release --project src\\MaHoaVaChuKySo.Console -- benchmark full

```



\### 5. Chạy benchmark RSA



Chạy kiểm thử nhanh:



```powershell

dotnet run -c Release --project src\\MaHoaVaChuKySo.Console -- rsabenchmark quick

```



Chạy kiểm thử đầy đủ:



```powershell

dotnet run -c Release --project src\\MaHoaVaChuKySo.Console -- rsabenchmark full

```



\---



\## Các chức năng và kết quả



\### 1. AES



Các chức năng mã hóa AES bao gồm:



\- Sinh khóa AES-256 gồm 32 byte ngẫu nhiên; lưu khóa dưới dạng Base64, nạp lại và kiểm tra khóa khớp.

\- Mã hóa văn bản bằng AES-GCM với nonce ngẫu nhiên 12 byte và tag 16 byte.

\- Định dạng dữ liệu đầu ra: `Base64(nonce | tag | ciphertext)`.

\- Kiểm thử mã hóa và giải mã cả văn bản thông thường lẫn văn bản rỗng.

\- Mã hóa file theo kiểu streaming với bộ đệm 128 KiB, không cần nạp toàn bộ file vào RAM.

\- Kiểm tra tính toàn vẹn của dữ liệu trước khi giải mã.



Định dạng file mã hóa riêng có tiền tố 54 byte:



| Thành phần | Kích thước |

| --- | ---: |

| Magic `MHCKS` | 5 byte |

| Version `0x01` | 1 byte |

| HMAC-SHA256 | 32 byte |

| IV | 16 byte |



Phần sau tiền tố là ciphertext AES-CBC sử dụng đệm PKCS7. Khóa mã hóa và khóa MAC được suy ra từ khóa chính bằng SHA-256 của nhãn cộng khóa chính.



Trước khi giải mã, chương trình kiểm tra HMAC bằng phép so sánh thời gian hằng số. File bị sửa hoặc sử dụng sai khóa sẽ bị từ chối trước bước giải mã.



\#### Kết quả benchmark AES



Số liệu đo trên máy của tác giả. Trong bảng này, 1 MB được tính bằng 1.000.000 byte. Thời gian chỉ tính riêng thao tác mã hóa và giải mã, không bao gồm việc tạo file kiểm thử, tính SHA-256 hoặc ghi CSV.



| Kích thước | Mã hóa | Giải mã | Toàn vẹn |

| --- | --- | --- | --- |

| 1 MB | 45,096 ms; 22,18 MB/s | 13,910 ms; 71,89 MB/s | PASS |

| 10 MB | 62,270 ms; 160,59 MB/s | 24,908 ms; 401,48 MB/s | PASS |

| 100 MB | 407,119 ms; 245,63 MB/s | 199,916 ms; 500,21 MB/s | PASS |

| 1000 MB | 5760,822 ms; 173,59 MB/s | 3408,780 ms; 293,36 MB/s | PASS |



Dữ liệu benchmark đầy đủ nằm tại:



`results/benchmark/aes-benchmark.csv`



\### 2. RSA và định dạng PEM



\#### Sinh khóa RSA



\- Sinh cặp khóa RSA-2048.

\- Xuất public key ở định dạng PEM SPKI, bắt đầu bằng `-----BEGIN PUBLIC KEY-----`.

\- Xuất private key ở định dạng PEM PKCS#8, bắt đầu bằng `-----BEGIN PRIVATE KEY-----`.

\- Nạp lại khóa để kiểm tra các thành phần và xác nhận tính chính xác của dữ liệu.



Theo kết quả kiểm thử:



\- Public key có `e = 65537`, modulus `n` dài 256 byte.

\- Private key có `e = 65537`, `n` dài 256 byte và `d` dài 256 byte.

\- Các thành phần `p`, `q`, `dp`, `dq` và `inverseQ` mỗi giá trị dài 128 byte.



\#### Mã hóa RSA



\- Sử dụng public key để mã hóa với OAEP-SHA256.

\- Với RSA-2048, modulus dài 256 byte.

\- Phần đệm OAEP-SHA256 chiếm 66 byte, vì vậy dữ liệu đầu vào tối đa là 190 byte.



Dự án triển khai demo mã hóa lai:



1\. Sinh khóa AES dài 32 byte.

2\. Sử dụng RSA public key để mã hóa khóa AES.

3\. Lưu ciphertext RSA dưới dạng Base64.

4\. Sử dụng private key để giải mã và khôi phục khóa AES.

5\. So sánh khóa được khôi phục với khóa ban đầu.



Kết quả kiểm thử cho thấy khóa AES được khôi phục chính xác. Dữ liệu dài 191 byte bị từ chối khi sử dụng cấu hình RSA-2048 và OAEP-SHA256.



\### 3. SHA-256 và hiệu ứng Avalanche



SHA-256 tạo ra giá trị băm có độ dài 256 bit, thường được biểu diễn bằng 64 ký tự hexadecimal.



Dự án thử nghiệm các đầu vào có nội dung khác nhau, bao gồm chuỗi rỗng, văn bản ngắn và văn bản tiếng Việt.



Bảng kết quả so sánh số bit khác nhau khi thay đổi dữ liệu đầu vào:



| Cặp đầu vào | Số bit khác / 256 |

| --- | ---: |

| `hello` / `hellp` | 131 (51,2%) |

| `Xin chao` / `Xin chbo` | 117 (45,7%) |

| `An toàn` / `An toan` | 144 (56,2%) |

| `(rỗng)` / `a` | 129 (50,4%) |

| `Ma hoa` / `Ma hoa` | 0 (0,0%) |



Các kết quả cho thấy việc thay đổi một phần nhỏ của dữ liệu đầu vào có thể làm thay đổi nhiều bit trong giá trị băm.



Dữ liệu đầy đủ nằm tại:



`results/experiments/sha256-avalanche.csv`



\### 4. Chữ ký số



Chữ ký số được sử dụng để kiểm tra tính xác thực và tính toàn vẹn của dữ liệu.



Quy trình cơ bản:



1\. Tính giá trị băm SHA-256 của dữ liệu.

2\. Sử dụng private key để tạo chữ ký số.

3\. Gửi dữ liệu cùng chữ ký cho bên nhận.

4\. Bên nhận sử dụng public key để xác minh chữ ký.

5\. Nếu dữ liệu bị thay đổi, quá trình xác minh sẽ thất bại.



Dự án so sánh hai phương pháp chữ ký:



\*\*RSA PKCS#1 v1.5\*\*



\- Có tính xác định trong cấu hình kiểm thử.

\- Cùng dữ liệu và cùng khóa sẽ tạo ra chữ ký giống nhau.

\- Với RSA-2048, chữ ký có độ dài 256 byte.



\*\*RSA-PSS\*\*



\- Sử dụng salt ngẫu nhiên trong quá trình ký.

\- Cùng dữ liệu và cùng khóa có thể tạo ra các chữ ký khác nhau.

\- Hai chữ ký PSS được kiểm thử đều xác minh thành công.

\- Khi dữ liệu bị sửa một ký tự, bước xác minh chữ ký bị từ chối.



\### 5. Benchmark RSA



Số liệu được đo trên máy của tác giả. Dữ liệu đem mã hóa là khóa AES 32 byte; thời gian sinh khóa không được tính vào phép đo.



| Kích thước khóa | Số lần | Mã hóa bằng public key | Giải mã bằng private key | Toàn vẹn |

| ---: | ---: | --- | --- | --- |

| 2048 bit | 50 | 1,566 ms; 31932,6 op/s | 69,276 ms; 721,7 op/s | PASS |

| 2048 bit | 500 | 16,799 ms; 29763,1 op/s | 755,818 ms; 661,5 op/s | PASS |

| 2048 bit | 5000 | 165,865 ms; 30145,0 op/s | 7075,074 ms; 706,7 op/s | PASS |

| 3072 bit | 500 | 39,396 ms; 12691,8 op/s | 1767,974 ms; 282,8 op/s | PASS |

| 3072 bit | 5000 | 315,194 ms; 15863,2 op/s | 17880,099 ms; 279,6 op/s | PASS |



Dữ liệu benchmark đầy đủ nằm tại:



`results/benchmark/rsa-benchmark.csv`



\---



\## Nhận xét từ kết quả đo



\- Trong các lần đo của dự án, AES giải mã nhanh hơn mã hóa. Kết quả ở file 1 MB có thể bị ảnh hưởng bởi chi phí khởi tạo trong lần chạy đầu tiên.

\- RSA mã hóa bằng public key nhanh hơn giải mã bằng private key trong các trường hợp đã đo.

\- Khóa RSA-3072 có thời gian xử lý dài hơn RSA-2048 trong các phép đo được ghi nhận, đặc biệt ở phía private key.

\- RSA-OAEP-SHA256 với khóa RSA-2048 chỉ mã hóa trực tiếp được tối đa 190 byte theo cấu hình thử nghiệm. Vì vậy, RSA được sử dụng để mã hóa khóa AES nhỏ, còn AES xử lý dữ liệu lớn.

\- RSA-PSS phù hợp để minh họa chữ ký số sử dụng salt ngẫu nhiên; PKCS#1 v1.5 tạo chữ ký xác định với cùng dữ liệu và khóa.



\## Lưu ý bảo mật



\- Các khóa và file PEM trong repository chỉ phục vụ mục đích kiểm thử, không sử dụng cho dữ liệu thật.

\- Các thư mục `keys/`, `test-data/` và những file `\*.pem`, `\*.key`, `\*.enc` được cấu hình trong `.gitignore` nhằm tránh đưa dữ liệu nhạy cảm lên GitHub.

\- Không commit private key hoặc khóa sử dụng trong môi trường thực tế vào repository.

\- Ảnh giao diện tại `docs/screenshots/main-ui.png` được mô tả là chỉ hiển thị tab Hash với giá trị băm công khai, không chứa private key hoặc tham số bí mật.

