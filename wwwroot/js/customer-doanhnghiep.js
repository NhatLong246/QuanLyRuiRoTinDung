$(document).ready(function() {
    // Override jQuery validation messages tiếng Việt NGAY LẬP TỨC
    if (typeof $.validator !== 'undefined' && $.validator.messages) {
        $.validator.messages.email = "Vui lòng nhập địa chỉ email hợp lệ.";
        $.validator.messages.required = "Trường này là bắt buộc.";
    }

    // Override cho tất cả required fields
    $('input').filter(function() { return $(this).attr('required'); }).each(function() {
        var $input = $(this);
        var fieldName = $input.attr('name');
        if (fieldName) {
            // Lấy message từ data attribute hoặc dùng default
            var customMessage = $input.attr('data-val-required');
            if (!customMessage) {
                // Tạo message mặc định dựa trên tên field
                var label = $input.closest('.form-group').find('label').text().replace('*', '').trim();
                customMessage = label + " là bắt buộc.";
                $input.attr('data-val-required', customMessage);
            }
            $input.attr('data-val', 'true');
        }
    });

    // Format số tiền với dấu chấm cho Doanh thu hàng năm
    const doanhThuInput = document.getElementById('doanhThuInput');
    const doanhThuHidden = document.getElementById('doanhThuHidden');
    
    if (doanhThuInput && doanhThuHidden) {
        doanhThuInput.addEventListener('input', function(e) {
            let value = e.target.value.replace(/\./g, '');
            if (value && !isNaN(value)) {
                let numValue = parseFloat(value);
                // Không cho phép số âm
                if (numValue < 0) {
                    numValue = 0;
                    value = '0';
                }
                let formatted = Math.floor(numValue).toLocaleString('vi-VN').replace(/,/g, '.');
                e.target.value = formatted;
                // Lưu giá trị số vào hidden field (không có dấu chấm)
                doanhThuHidden.value = Math.floor(numValue).toString();
                
                // Xóa lỗi nếu có
                const errorSpan = this.parentElement.querySelector('.validation-error');
                if (errorSpan) {
                    errorSpan.textContent = '';
                    errorSpan.style.display = 'none';
                }
            } else {
                doanhThuHidden.value = '';
            }
        });

        doanhThuInput.addEventListener('blur', function() {
            let value = this.value.replace(/\./g, '');
            const errorSpan = this.parentElement.querySelector('.validation-error');
            if (value && !isNaN(value)) {
                let numValue = parseFloat(value);
                if (numValue < 0) {
                    if (errorSpan) {
                        errorSpan.textContent = 'Doanh thu hàng năm không được âm.';
                        errorSpan.style.display = 'block';
                    }
                    this.value = '0';
                    doanhThuHidden.value = '0';
                } else {
                    if (errorSpan) {
                        errorSpan.textContent = '';
                        errorSpan.style.display = 'none';
                    }
                }
            }
        });

        document.querySelector('form')?.addEventListener('submit', function(e) {
            if (doanhThuInput.value) {
                let value = doanhThuInput.value.replace(/\./g, '');
                let numValue = parseFloat(value);
                if (numValue < 0) {
                    e.preventDefault();
                    const errorSpan = doanhThuInput.parentElement.querySelector('.validation-error');
                    if (errorSpan) {
                        errorSpan.textContent = 'Doanh thu hàng năm không được âm.';
                        errorSpan.style.display = 'block';
                    }
                    doanhThuInput.focus();
                    return false;
                }
                doanhThuHidden.value = Math.floor(numValue).toString();
            }
        });
    }

    // Format số tiền với dấu chấm cho Tổng tài sản
    const tongTaiSanInput = document.getElementById('tongTaiSanInput');
    const tongTaiSanHidden = document.getElementById('tongTaiSanHidden');
    
    if (tongTaiSanInput && tongTaiSanHidden) {
        tongTaiSanInput.addEventListener('input', function(e) {
            let value = e.target.value.replace(/\./g, '');
            if (value && !isNaN(value)) {
                let numValue = parseFloat(value);
                // Không cho phép số âm
                if (numValue < 0) {
                    numValue = 0;
                    value = '0';
                }
                let formatted = Math.floor(numValue).toLocaleString('vi-VN').replace(/,/g, '.');
                e.target.value = formatted;
                // Lưu giá trị số vào hidden field (không có dấu chấm)
                tongTaiSanHidden.value = Math.floor(numValue).toString();
                
                // Xóa lỗi nếu có
                const errorSpan = this.parentElement.querySelector('.validation-error');
                if (errorSpan) {
                    errorSpan.textContent = '';
                    errorSpan.style.display = 'none';
                }
            } else {
                tongTaiSanHidden.value = '';
            }
        });

        tongTaiSanInput.addEventListener('blur', function() {
            let value = this.value.replace(/\./g, '');
            const errorSpan = this.parentElement.querySelector('.validation-error');
            if (value && !isNaN(value)) {
                let numValue = parseFloat(value);
                if (numValue < 0) {
                    if (errorSpan) {
                        errorSpan.textContent = 'Tổng tài sản không được âm.';
                        errorSpan.style.display = 'block';
                    }
                    this.value = '0';
                    tongTaiSanHidden.value = '0';
                } else {
                    if (errorSpan) {
                        errorSpan.textContent = '';
                        errorSpan.style.display = 'none';
                    }
                }
            }
        });

        document.querySelector('form')?.addEventListener('submit', function(e) {
            if (tongTaiSanInput.value) {
                let value = tongTaiSanInput.value.replace(/\./g, '');
                let numValue = parseFloat(value);
                if (numValue < 0) {
                    e.preventDefault();
                    const errorSpan = tongTaiSanInput.parentElement.querySelector('.validation-error');
                    if (errorSpan) {
                        errorSpan.textContent = 'Tổng tài sản không được âm.';
                        errorSpan.style.display = 'block';
                    }
                    tongTaiSanInput.focus();
                    return false;
                }
                tongTaiSanHidden.value = Math.floor(numValue).toString();
            }
        });
    }

    // Format số tiền với dấu chấm cho Vốn điều lệ
    const vonDieuLeInput = document.getElementById('vonDieuLeInput');
    const vonDieuLeHidden = document.getElementById('vonDieuLeHidden');
    
    if (vonDieuLeInput && vonDieuLeHidden) {
        vonDieuLeInput.addEventListener('input', function(e) {
            let value = e.target.value.replace(/\./g, '');
            if (value && !isNaN(value)) {
                let numValue = parseFloat(value);
                // Không cho phép số âm
                if (numValue < 0) {
                    numValue = 0;
                    value = '0';
                }
                let formatted = Math.floor(numValue).toLocaleString('vi-VN').replace(/,/g, '.');
                e.target.value = formatted;
                // Lưu giá trị số vào hidden field (không có dấu chấm)
                vonDieuLeHidden.value = Math.floor(numValue).toString();
                
                // Xóa lỗi nếu có
                const errorSpan = this.parentElement.querySelector('.validation-error');
                if (errorSpan) {
                    errorSpan.textContent = '';
                    errorSpan.style.display = 'none';
                }
            } else {
                vonDieuLeHidden.value = '';
            }
        });

        vonDieuLeInput.addEventListener('blur', function() {
            let value = this.value.replace(/\./g, '');
            const errorSpan = this.parentElement.querySelector('.validation-error');
            if (value && !isNaN(value)) {
                let numValue = parseFloat(value);
                if (numValue < 0) {
                    if (errorSpan) {
                        errorSpan.textContent = 'Vốn điều lệ không được âm.';
                        errorSpan.style.display = 'block';
                    }
                    this.value = '0';
                    vonDieuLeHidden.value = '0';
                } else {
                    if (errorSpan) {
                        errorSpan.textContent = '';
                        errorSpan.style.display = 'none';
                    }
                }
            }
        });

        document.querySelector('form')?.addEventListener('submit', function(e) {
            if (vonDieuLeInput.value) {
                let value = vonDieuLeInput.value.replace(/\./g, '');
                let numValue = parseFloat(value);
                if (numValue < 0) {
                    e.preventDefault();
                    const errorSpan = vonDieuLeInput.parentElement.querySelector('.validation-error');
                    if (errorSpan) {
                        errorSpan.textContent = 'Vốn điều lệ không được âm.';
                        errorSpan.style.display = 'block';
                    }
                    vonDieuLeInput.focus();
                    return false;
                }
                vonDieuLeHidden.value = Math.floor(numValue).toString();
            }
        });
    }

    // Format và validate Mã số thuế: 0123456789-001 (10 số + 3 số phụ)
    const maSoThueInput = document.getElementById('maSoThueInput');
    const maSoThueHidden = document.getElementById('maSoThueHidden');
    const maSoThueError = document.getElementById('maSoThueError');
    
    if (maSoThueInput && maSoThueHidden) {
        // Hàm lấy số thuần từ input
        function getNumbersOnly(value) {
            return value.replace(/[^\d]/g, '');
        }
        
        // Hàm format mã số thuế
        function formatMaSoThue(numbersOnly) {
            if (numbersOnly.length > 10) {
                let part1 = numbersOnly.substring(0, 10);
                let part2 = numbersOnly.substring(10, 13); // Chỉ lấy tối đa 3 số
                return part1 + '-' + part2;
            }
            return numbersOnly;
        }
        
        // Hàm validate và hiển thị lỗi
        function validateMaSoThue() {
            let inputValue = maSoThueInput.value;
            let numbersOnly = getNumbersOnly(inputValue);
            
            // Giới hạn 13 chữ số
            if (numbersOnly.length > 13) {
                numbersOnly = numbersOnly.substring(0, 13);
            }
            
            // Format lại input
            if (numbersOnly.length > 0) {
                maSoThueInput.value = formatMaSoThue(numbersOnly);
            }
            
            // Lưu vào hidden field
            maSoThueHidden.value = numbersOnly;
            
            // Validate
            if (numbersOnly.length === 13) {
                // Đủ 13 số - hợp lệ
                if (maSoThueError) {
                    maSoThueError.textContent = '';
                    maSoThueError.style.display = 'none';
                }
                return true;
            } else if (numbersOnly.length > 0) {
                // Có số nhưng chưa đủ 13
                if (maSoThueError) {
                    maSoThueError.textContent = 'Mã số thuế phải đủ 13 chữ số (10 chữ số + 3 chữ số phụ).';
                    maSoThueError.style.display = 'block';
                }
                return false;
            } else {
                // Rỗng
                if (maSoThueError) {
                    maSoThueError.textContent = '';
                    maSoThueError.style.display = 'none';
                }
                return false;
            }
        }
        
        // Xử lý khi nhập
        maSoThueInput.addEventListener('input', function(e) {
            // Chỉ cho phép số và dấu -
            let value = e.target.value.replace(/[^\d-]/g, '');
            
            // Không cho phép nhiều dấu -
            let parts = value.split('-');
            if (parts.length > 2) {
                value = parts[0] + '-' + parts.slice(1).join('');
            }
            
            e.target.value = value;
            validateMaSoThue();
        });
        
        // Xử lý khi blur
        maSoThueInput.addEventListener('blur', function() {
            let isValid = validateMaSoThue();
            
            // Nếu hợp lệ (13 số), kiểm tra trùng
            if (isValid && maSoThueHidden.value && maSoThueHidden.value.length === 13) {
                checkMaSoThueExists(maSoThueHidden.value);
            }
        });
        
        // Xử lý khi paste
        maSoThueInput.addEventListener('paste', function(e) {
            setTimeout(function() {
                validateMaSoThue();
            }, 10);
        });
    }
    
    // Kiểm tra mã số thuế trùng
    async function checkMaSoThueExists(maSoThue) {
        try {
            const response = await fetch(`/Customer/CheckMaSoThueExists?maSoThue=${encodeURIComponent(maSoThue)}`);
            const data = await response.json();
            
            const errorSpan = document.getElementById('maSoThueError');
            if (data.exists) {
                if (errorSpan) {
                    errorSpan.textContent = 'Mã số thuế này đã được sử dụng bởi doanh nghiệp khác.';
                    errorSpan.style.display = 'block';
                }
            } else {
                if (errorSpan && errorSpan.textContent.includes('đã được sử dụng')) {
                    errorSpan.textContent = '';
                    errorSpan.style.display = 'none';
                }
            }
        } catch (error) {
            console.error('Error checking MaSoThue:', error);
        }
    }
    
    // Validation client-side cho số điện thoại
    const soDienThoaiInput = document.querySelector('input[name="SoDienThoai"]');
    if (soDienThoaiInput) {
        soDienThoaiInput.addEventListener('input', function(e) {
            let value = e.target.value.replace(/\D/g, '');
            if (value.length > 10) value = value.substring(0, 10);
            e.target.value = value;
            
            // Xóa lỗi khi nhập
            const errorSpan = this.parentElement.querySelector('.validation-error');
            if (errorSpan && value.length === 10) {
                errorSpan.textContent = '';
                errorSpan.style.display = 'none';
            }
        });
        
        soDienThoaiInput.addEventListener('blur', function() {
            const value = this.value.replace(/\D/g, '');
            const errorSpan = this.parentElement.querySelector('.validation-error');
            
            if (value.length > 0 && value.length !== 10) {
                if (errorSpan) {
                    errorSpan.textContent = 'Số điện thoại phải đủ 10 chữ số.';
                    errorSpan.style.display = 'block';
                }
            } else if (value.length === 10) {
                // Kiểm tra trùng qua AJAX
                checkPhoneExists(value);
            }
        });
    }
    
    // Kiểm tra số điện thoại trùng
    async function checkPhoneExists(phone) {
        try {
            const response = await fetch(`/Customer/CheckPhoneExists?phone=${encodeURIComponent(phone)}`);
            const data = await response.json();
            
            const errorSpan = soDienThoaiInput.parentElement.querySelector('.validation-error');
            if (data.exists) {
                if (errorSpan) {
                    errorSpan.textContent = 'Số điện thoại này đã được sử dụng bởi khách hàng khác.';
                    errorSpan.style.display = 'block';
                }
            } else {
                if (errorSpan && errorSpan.textContent.includes('đã được sử dụng')) {
                    errorSpan.textContent = '';
                    errorSpan.style.display = 'none';
                }
            }
        } catch (error) {
            console.error('Error checking phone:', error);
        }
    }

    // Đảm bảo email validation message là tiếng Việt và kiểm tra trùng
    const emailInput = document.querySelector('input[name="Email"]');
    if (emailInput) {
        emailInput.addEventListener('invalid', function(e) {
            if (e.target.validity.typeMismatch) {
                e.target.setCustomValidity('Vui lòng nhập địa chỉ email hợp lệ.');
            } else {
                e.target.setCustomValidity('');
            }
        });

        emailInput.addEventListener('input', function(e) {
            e.target.setCustomValidity('');
            // Xóa lỗi khi nhập
            const errorSpan = this.parentElement.querySelector('.validation-error');
            if (errorSpan && e.target.validity.valid) {
                errorSpan.textContent = '';
                errorSpan.style.display = 'none';
            }
        });
        
        emailInput.addEventListener('blur', function() {
            const email = this.value.trim();
            const errorSpan = this.parentElement.querySelector('.validation-error');
            
            if (email.length > 0) {
                // Kiểm tra định dạng email
                const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
                if (!emailRegex.test(email)) {
                    if (errorSpan) {
                        errorSpan.textContent = 'Email không đúng định dạng.';
                        errorSpan.style.display = 'block';
                    }
                } else {
                    // Kiểm tra trùng qua AJAX
                    checkEmailExists(email);
                }
            }
        });
    }
    
    // Kiểm tra email trùng
    async function checkEmailExists(email) {
        try {
            const response = await fetch(`/Customer/CheckEmailExists?email=${encodeURIComponent(email)}`);
            const data = await response.json();
            
            const errorSpan = emailInput.parentElement.querySelector('.validation-error');
            if (data.exists) {
                if (errorSpan) {
                    errorSpan.textContent = 'Email này đã được sử dụng bởi khách hàng khác.';
                    errorSpan.style.display = 'block';
                }
            } else {
                if (errorSpan && (errorSpan.textContent.includes('đã được sử dụng') || errorSpan.textContent.includes('không đúng định dạng'))) {
                    errorSpan.textContent = '';
                    errorSpan.style.display = 'none';
                }
            }
        } catch (error) {
            console.error('Error checking email:', error);
        }
    }
    
    // Validate ngày cấp giấy phép phải trước ngày đăng ký thành lập
    const ngayCapGiayPhep = document.getElementById('ngayCapGiayPhep');
    const ngayDangKy = document.getElementById('ngayDangKy');
    
    if (ngayCapGiayPhep && ngayDangKy) {
        // Set max date là hôm nay
        const today = new Date().toISOString().split('T')[0];
        ngayCapGiayPhep.setAttribute('max', today);
        ngayDangKy.setAttribute('max', today);
        
        ngayCapGiayPhep.addEventListener('change', function() {
            validateNgayCapVsNgayDangKy();
        });
        
        ngayDangKy.addEventListener('change', function() {
            validateNgayCapVsNgayDangKy();
        });
    }
    
    function validateNgayCapVsNgayDangKy() {
        const ngayCap = ngayCapGiayPhep.value;
        const ngayDangKyValue = ngayDangKy.value;
        const errorSpanNgayCap = ngayCapGiayPhep.parentElement.querySelector('.validation-error');
        const errorSpanNgayDangKy = ngayDangKy.parentElement.querySelector('.validation-error');
        
        if (ngayCap && ngayDangKyValue) {
            const dateCap = new Date(ngayCap);
            const dateDangKy = new Date(ngayDangKyValue);
            
            if (dateCap > dateDangKy) {
                if (errorSpanNgayCap) {
                    errorSpanNgayCap.textContent = 'Ngày cấp giấy phép phải trước hoặc bằng ngày đăng ký thành lập.';
                    errorSpanNgayCap.style.display = 'block';
                }
                if (errorSpanNgayDangKy) {
                    errorSpanNgayDangKy.textContent = 'Ngày đăng ký thành lập phải sau hoặc bằng ngày cấp giấy phép.';
                    errorSpanNgayDangKy.style.display = 'block';
                }
            } else {
                if (errorSpanNgayCap) {
                    errorSpanNgayCap.textContent = '';
                    errorSpanNgayCap.style.display = 'none';
                }
                if (errorSpanNgayDangKy) {
                    errorSpanNgayDangKy.textContent = '';
                    errorSpanNgayDangKy.style.display = 'none';
                }
            }
        }
    }
    
    // Validate CCCD người đại diện pháp luật
    const cccdInput = document.querySelector('input[name="SoCccdNguoiDaiDienPhapLuat"]');
    if (cccdInput) {
        cccdInput.addEventListener('input', function(e) {
            let value = e.target.value.replace(/\D/g, '');
            if (value.length > 12) value = value.substring(0, 12);
            e.target.value = value;
            
            // Xóa lỗi khi nhập
            const errorSpan = this.parentElement.querySelector('.validation-error');
            if (errorSpan && value.length === 12) {
                errorSpan.textContent = '';
                errorSpan.style.display = 'none';
            }
        });
        
        cccdInput.addEventListener('blur', function() {
            const value = this.value.replace(/\D/g, '');
            const errorSpan = this.parentElement.querySelector('.validation-error');
            
            if (value.length > 0 && value.length !== 12) {
                if (errorSpan) {
                    errorSpan.textContent = 'Số CCCD phải đủ 12 chữ số.';
                    errorSpan.style.display = 'block';
                }
            }
        });
    }

    // Validate số lượng nhân viên không được âm
    const soLuongNhanVien = document.getElementById('soLuongNhanVien');
    if (soLuongNhanVien) {
        soLuongNhanVien.addEventListener('input', function(e) {
            let value = parseFloat(e.target.value);
            if (isNaN(value) || value < 0) {
                e.target.value = '';
                const errorSpan = this.parentElement.querySelector('.validation-error');
                if (errorSpan) {
                    errorSpan.textContent = 'Số lượng nhân viên không được âm.';
                    errorSpan.style.display = 'block';
                }
            } else {
                // Đảm bảo là số nguyên
                e.target.value = Math.floor(value);
                const errorSpan = this.parentElement.querySelector('.validation-error');
                if (errorSpan) {
                    errorSpan.textContent = '';
                    errorSpan.style.display = 'none';
                }
            }
        });

        soLuongNhanVien.addEventListener('keydown', function(e) {
            // Ngăn nhập dấu trừ và dấu chấm
            if (e.key === '-' || e.key === '.' || e.key === 'e' || e.key === 'E' || e.key === '+') {
                e.preventDefault();
            }
        });

        soLuongNhanVien.addEventListener('blur', function() {
            let value = parseFloat(this.value);
            const errorSpan = this.parentElement.querySelector('.validation-error');
            
            if (this.value && (isNaN(value) || value < 0)) {
                if (errorSpan) {
                    errorSpan.textContent = 'Số lượng nhân viên không được âm.';
                    errorSpan.style.display = 'block';
                }
            } else if (this.value && value >= 0) {
                // Đảm bảo là số nguyên
                this.value = Math.floor(value);
                if (errorSpan) {
                    errorSpan.textContent = '';
                    errorSpan.style.display = 'none';
                }
            }
        });
    }

    // Validate ngày sinh người đại diện phải đủ 18 tuổi
    const ngaySinhNguoiDaiDien = document.getElementById('ngaySinhNguoiDaiDien');
    if (ngaySinhNguoiDaiDien) {
        // Set max date là 18 năm trước
        const today = new Date();
        const maxDate = new Date(today.getFullYear() - 18, today.getMonth(), today.getDate());
        ngaySinhNguoiDaiDien.setAttribute('max', maxDate.toISOString().split('T')[0]);
        
        ngaySinhNguoiDaiDien.addEventListener('change', function() {
            const selectedDate = new Date(this.value);
            const today = new Date();
            const age = today.getFullYear() - selectedDate.getFullYear();
            const monthDiff = today.getMonth() - selectedDate.getMonth();
            const dayDiff = today.getDate() - selectedDate.getDate();
            
            let actualAge = age;
            if (monthDiff < 0 || (monthDiff === 0 && dayDiff < 0)) {
                actualAge--;
            }
            
            const errorSpan = this.parentElement.querySelector('.validation-error');
            if (actualAge < 18) {
                if (errorSpan) {
                    errorSpan.textContent = 'Người đại diện pháp luật phải đủ 18 tuổi.';
                    errorSpan.style.display = 'block';
                }
            } else {
                if (errorSpan) {
                    errorSpan.textContent = '';
                    errorSpan.style.display = 'none';
                }
            }
        });
        
        ngaySinhNguoiDaiDien.addEventListener('blur', function() {
            if (this.value) {
                const selectedDate = new Date(this.value);
                const today = new Date();
                const age = today.getFullYear() - selectedDate.getFullYear();
                const monthDiff = today.getMonth() - selectedDate.getMonth();
                const dayDiff = today.getDate() - selectedDate.getDate();
                
                let actualAge = age;
                if (monthDiff < 0 || (monthDiff === 0 && dayDiff < 0)) {
                    actualAge--;
                }
                
                const errorSpan = this.parentElement.querySelector('.validation-error');
                if (actualAge < 18) {
                    if (errorSpan) {
                        errorSpan.textContent = 'Người đại diện pháp luật phải đủ 18 tuổi.';
                        errorSpan.style.display = 'block';
                    }
                } else {
                    if (errorSpan) {
                        errorSpan.textContent = '';
                        errorSpan.style.display = 'none';
                    }
                }
            }
        });
    }

    // Sau khi unobtrusive validation load, override lại messages
    setTimeout(function() {
        if (typeof $.validator !== 'undefined' && $.validator.messages) {
            $.validator.messages.email = "Vui lòng nhập địa chỉ email hợp lệ.";
            $.validator.messages.required = "Trường này là bắt buộc.";
            $.validator.messages.number = "Vui lòng nhập số hợp lệ.";
            $.validator.messages.maxlength = $.validator.format("Vui lòng nhập tối đa {0} ký tự.");
            $.validator.messages.minlength = $.validator.format("Vui lòng nhập tối thiểu {0} ký tự.");
        }
        
        // Override cho tất cả required fields
        $('input').filter(function() { return $(this).attr('required'); }).each(function() {
            var $input = $(this);
            var fieldName = $input.attr('name');
            if (fieldName && !$input.attr('data-val-required')) {
                var label = $input.closest('.form-group').find('label').text().replace('*', '').trim();
                var customMessage = label + " là bắt buộc.";
                $input.attr('data-val-required', customMessage);
                $input.attr('data-val', 'true');
            }
        });
        
        $('input').filter(function() { return $(this).attr('type') === 'email'; }).each(function() {
            var $input = $(this);
            $input.attr('data-val-email', 'Vui lòng nhập địa chỉ email hợp lệ.');
            $input.attr('data-msg-email', 'Vui lòng nhập địa chỉ email hợp lệ.');
            
            var form = $input.closest('form');
            if (form.length) {
                var validator = form.data('validator');
                if (validator && validator.settings && validator.settings.messages) {
                    var fieldName = $input.attr('name');
                    if (fieldName) {
                        if (!validator.settings.messages[fieldName]) {
                            validator.settings.messages[fieldName] = {};
                        }
                        validator.settings.messages[fieldName].email = "Vui lòng nhập địa chỉ email hợp lệ.";
                    }
                }
            }
        });
    }, 500);
    
    // Xử lý submit form - đảm bảo mã số thuế được lưu đúng (13 chữ số, không có dấu -)
    document.querySelector('form')?.addEventListener('submit', function(e) {
        // Validate lại mã số thuế trước khi submit
        if (maSoThueInput && maSoThueHidden) {
            // Validate lại
            let isValid = validateMaSoThue();
            
            if (!isValid || !maSoThueHidden.value || maSoThueHidden.value.length !== 13) {
                e.preventDefault();
                e.stopPropagation();
                
                const errorSpan = document.getElementById('maSoThueError');
                if (errorSpan) {
                    if (!maSoThueInput.value || maSoThueInput.value.trim() === '') {
                        errorSpan.textContent = 'Mã số thuế là bắt buộc.';
                    } else {
                        errorSpan.textContent = 'Mã số thuế phải đủ 13 chữ số (10 chữ số + 3 chữ số phụ).';
                    }
                    errorSpan.style.display = 'block';
                }
                
                maSoThueInput.focus();
                return false;
            }
            
            // Đảm bảo hidden field có giá trị số thuần (13 chữ số)
            maSoThueHidden.value = maSoThueHidden.value.replace(/[^\d]/g, '').substring(0, 13);
        }
    });
});
