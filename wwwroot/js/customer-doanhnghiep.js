$(document).ready(function() {
    // Disable jQuery validation hoàn toàn để dùng custom validation
    if (typeof $.validator !== 'undefined') {
        $.validator.setDefaults({
            ignore: '*' // Ignore all fields - disable jQuery validation
        });
    }

    // Custom realtime validation cho TẤT CẢ các trường required
    $('input[required], select[required], textarea[required]').each(function() {
        const $input = $(this);
        const errorSpan = $input.parent().find('.validation-error')[0] || 
                         $input.siblings('.validation-error')[0];
        
        // Validate ngay khi nhập
        $input.on('input change', function() {
            const value = this.value.trim();
            const fieldName = $input.parent().find('label').text().replace('*', '').trim();
            
            if (!value || value === '') {
                if (errorSpan) {
                    errorSpan.textContent = fieldName + ' là bắt buộc.';
                    errorSpan.style.display = 'block';
                }
            } else {
                if (errorSpan && errorSpan.textContent.includes('là bắt buộc')) {
                    errorSpan.textContent = '';
                    errorSpan.style.display = 'none';
                }
            }
        });
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
    
    // Kiểm tra mã số thuế trùng và cross-validate với CIC
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
                
                // Cross-validate với CIC nếu không trùng
                await checkCrossValidationCIC();
            }
        } catch (error) {
            console.error('Error checking MaSoThue:', error);
        }
    }
    
    // Kiểm tra giấy phép kinh doanh trùng - REALTIME
    const giayPhepInput = document.getElementById('giayPhepInput');
    let giayPhepTimeout = null;
    if (giayPhepInput) {
        giayPhepInput.addEventListener('input', async function() {
            const value = this.value.trim();
            const errorSpan = document.getElementById('giayPhepError');
            
            // Debounce để tránh gọi API quá nhiều
            if (giayPhepTimeout) clearTimeout(giayPhepTimeout);
            
            if (value && value.length >= 3) {
                giayPhepTimeout = setTimeout(async () => {
                    try {
                        const response = await fetch(`/Customer/CheckGiayPhepExists?soGiayPhep=${encodeURIComponent(value)}`);
                        const data = await response.json();
                        
                        if (data.exists) {
                            if (errorSpan) {
                                errorSpan.textContent = 'Số giấy phép kinh doanh này đã được sử dụng bởi doanh nghiệp khác.';
                                errorSpan.style.display = 'block';
                            }
                        } else {
                            if (errorSpan && errorSpan.textContent.includes('đã được sử dụng')) {
                                errorSpan.textContent = '';
                                errorSpan.style.display = 'none';
                            }
                        }
                    } catch (error) {
                        console.error('Error checking GiayPhep:', error);
                    }
                }, 300); // 300ms debounce
            } else {
                if (errorSpan && errorSpan.textContent.includes('đã được sử dụng')) {
                    errorSpan.textContent = '';
                    errorSpan.style.display = 'none';
                }
            }
        });
    }
    
    // Kiểm tra Tên công ty ngay khi nhập - REALTIME với check trùng
    const tenCongTyInput = document.getElementById('tenCongTyInput');
    let tenCongTyTimeout = null;
    if (tenCongTyInput) {
        tenCongTyInput.addEventListener('input', async function() {
            const value = this.value.trim();
            const errorSpan = document.getElementById('tenCongTyError');
            
            // Debounce để tránh gọi API quá nhiều
            if (tenCongTyTimeout) clearTimeout(tenCongTyTimeout);
            
            // Hiển thị lỗi nếu rỗng
            if (!value) {
                if (errorSpan) {
                    errorSpan.textContent = 'Tên công ty là bắt buộc.';
                    errorSpan.style.display = 'block';
                }
            } else {
                // Xóa lỗi required nếu có
                if (errorSpan && errorSpan.textContent === 'Tên công ty là bắt buộc.') {
                    errorSpan.textContent = '';
                    errorSpan.style.display = 'none';
                }
                
                // Check trùng tên công ty sau 300ms
                if (value.length >= 2) {
                    tenCongTyTimeout = setTimeout(async () => {
                        try {
                            const response = await fetch(`/Customer/CheckTenCongTyExists?tenCongTy=${encodeURIComponent(value)}`);
                            const data = await response.json();
                            
                            if (data.exists) {
                                if (errorSpan) {
                                    errorSpan.textContent = 'Tên công ty này đã được sử dụng bởi doanh nghiệp khác.';
                                    errorSpan.style.display = 'block';
                                }
                            } else {
                                if (errorSpan && errorSpan.textContent.includes('đã được sử dụng')) {
                                    errorSpan.textContent = '';
                                    errorSpan.style.display = 'none';
                                }
                                // Cross-validate với CIC
                                await checkCrossValidationCIC();
                            }
                        } catch (error) {
                            console.error('Error checking TenCongTy:', error);
                        }
                    }, 300);
                }
            }
        });
    }
    
    // Hàm cross-validation với CIC cho Mã số thuế, Tên công ty và CCCD người đại diện
    async function checkCrossValidationCIC() {
        const maSoThue = document.getElementById('maSoThueHidden')?.value;
        const tenCongTy = document.getElementById('tenCongTyInput')?.value.trim();
        const cccdNguoiDaiDien = document.getElementById('cccdInput')?.value.trim();
        
        // Chỉ kiểm tra khi có đủ thông tin
        if (!maSoThue || maSoThue.length !== 13) return;
        if (!tenCongTy) return;
        
        try {
            const response = await fetch(`/Customer/CrossValidateCIC?maSoThue=${encodeURIComponent(maSoThue)}&tenCongTy=${encodeURIComponent(tenCongTy)}&cccd=${encodeURIComponent(cccdNguoiDaiDien || '')}`);
            const data = await response.json();
            
            // Xóa tất cả warning cũ
            hideAllCrossWarnings();
            
            if (data.hasCicRecord) {
                // Kiểm tra Mã số thuế
                if (data.maSoThueMismatch) {
                    showCrossWarning('maSoThueInput', 'Mã số thuế không khớp với thông tin trong CIC.');
                }
                
                // Kiểm tra Tên công ty
                if (data.tenCongTyMismatch) {
                    showCrossWarning('tenCongTyInput', 'Tên công ty không khớp với thông tin trong CIC.');
                }
                
                // Kiểm tra CCCD người đại diện
                if (cccdNguoiDaiDien && data.cccdMismatch) {
                    showCrossWarning('cccdInput', 'CCCD người đại diện không khớp với thông tin trong CIC.');
                }
            }
        } catch (error) {
            console.error('Error cross-validating with CIC:', error);
        }
    }
    
    // Hàm hiển thị warning label cho cross-validation
    function showCrossWarning(inputId, message) {
        const input = document.getElementById(inputId);
        if (!input) return;
        
        // Tìm hoặc tạo warning label
        let warningLabel = input.parentElement.querySelector('.cross-validation-warning');
        if (!warningLabel) {
            warningLabel = document.createElement('span');
            warningLabel.className = 'cross-validation-warning';
            warningLabel.style.color = '#ff9800';
            warningLabel.style.fontSize = '0.875rem';
            warningLabel.style.display = 'block';
            warningLabel.style.marginTop = '4px';
            input.parentElement.appendChild(warningLabel);
        }
        
        warningLabel.textContent = '⚠ ' + message;
        warningLabel.style.display = 'block';
    }
    
    function hideAllCrossWarnings() {
        const warnings = document.querySelectorAll('.cross-validation-warning');
        warnings.forEach(w => w.remove());
    }
    
    // Validation client-side cho số điện thoại
    const soDienThoaiInput = document.querySelector('input[name="SoDienThoai"]');
    if (soDienThoaiInput) {
        soDienThoaiInput.addEventListener('input', function(e) {
            let value = e.target.value.replace(/\D/g, '');
            if (value.length > 10) value = value.substring(0, 10);
            e.target.value = value;
            
            const errorSpan = this.parentElement.querySelector('.validation-error');
            
            // Validate ngay khi nhập
            if (value.length > 0 && value.length < 10) {
                if (errorSpan) {
                    errorSpan.textContent = 'Số điện thoại phải đủ 10 chữ số.';
                    errorSpan.style.display = 'block';
                }
            } else if (value.length === 10) {
                if (errorSpan) {
                    errorSpan.textContent = '';
                    errorSpan.style.display = 'none';
                }
                // Kiểm tra trùng qua AJAX khi đủ 10 số
                checkPhoneExists(value);
            } else {
                if (errorSpan) {
                    errorSpan.textContent = '';
                    errorSpan.style.display = 'none';
                }
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
            const email = this.value.trim();
            const errorSpan = this.parentElement.querySelector('.validation-error');
            
            // Validate realtime khi nhập
            if (email.length > 0) {
                const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
                if (!emailRegex.test(email)) {
                    if (errorSpan) {
                        errorSpan.textContent = 'Email không đúng định dạng.';
                        errorSpan.style.display = 'block';
                    }
                } else {
                    if (errorSpan) {
                        errorSpan.textContent = '';
                        errorSpan.style.display = 'none';
                    }
                    // Kiểm tra trùng khi email hợp lệ
                    checkEmailExists(email);
                }
            } else {
                if (errorSpan) {
                    errorSpan.textContent = '';
                    errorSpan.style.display = 'none';
                }
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
    
    // Validate CCCD người đại diện pháp luật và auto-fill
    const cccdInput = document.getElementById('cccdNguoiDaiDienInput') || document.querySelector('input[name="SoCccdNguoiDaiDienPhapLuat"]');
    const nguoiDaiDienInput = document.getElementById('nguoiDaiDienInput') || document.querySelector('input[name="NguoiDaiDienPhapLuat"]');
    const ngaySinhInput = document.getElementById('ngaySinhNguoiDaiDienInput') || document.querySelector('input[name="NgaySinh"]');
    const gioiTinhInput = document.getElementById('gioiTinhInput') || document.querySelector('select[name="GioiTinh"]');
    
    if (cccdInput) {
        cccdInput.addEventListener('input', function(e) {
            let value = e.target.value.replace(/\D/g, '');
            if (value.length > 12) value = value.substring(0, 12);
            e.target.value = value;
            
            const errorSpan = this.parentElement.querySelector('.validation-error');
            
            // Validate realtime khi nhập
            if (value.length > 0 && value.length < 12) {
                if (errorSpan) {
                    errorSpan.textContent = 'Số CCCD phải đủ 12 chữ số.';
                    errorSpan.style.display = 'block';
                }
            } else if (value.length === 12) {
                if (errorSpan) {
                    errorSpan.textContent = '';
                    errorSpan.style.display = 'none';
                }
                // Auto-fill thông tin từ CCCD
                checkAndAutoFillFromCccd(value);
            } else {
                if (errorSpan) {
                    errorSpan.textContent = '';
                    errorSpan.style.display = 'none';
                }
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
            } else if (value.length === 12) {
                checkAndAutoFillFromCccd(value);
            }
        });
    }
    
    // Hàm kiểm tra và tự động điền thông tin từ CCCD + CROSS-VALIDATION NGAY LẬP TỨC
    async function checkAndAutoFillFromCccd(soCccd) {
        try {
            const response = await fetch(`/Customer/GetInfoFromCccd?soCccd=${encodeURIComponent(soCccd)}`);
            const data = await response.json();
            
            // Xóa tất cả warning cũ trước
            hideAllCrossWarnings();
            
            if (data.exists) {
                // Kiểm tra nếu đã có giá trị trong form, so sánh và hiển thị lỗi NGAY
                const currentNguoiDaiDien = nguoiDaiDienInput?.value?.trim() || '';
                const currentNgaySinh = ngaySinhInput?.value || '';
                const currentGioiTinh = gioiTinhInput?.value || '';
                
                // Nếu form đã có data, so sánh và báo lỗi NGAY nếu không khớp
                if (currentNguoiDaiDien && data.hoTen && currentNguoiDaiDien.toLowerCase() !== data.hoTen.toLowerCase()) {
                    showCrossWarningNguoiDaiDien('NguoiDaiDienPhapLuat', 
                        `Người đại diện pháp luật phải giống với thông tin khách hàng cá nhân đã đăng ký với CCCD này. Giá trị hiện tại: ${data.hoTen}`);
                }
                
                if (currentNgaySinh && data.ngaySinh && currentNgaySinh !== data.ngaySinh) {
                    showCrossWarningNguoiDaiDien('NgaySinh', 
                        `Ngày sinh phải giống với thông tin khách hàng cá nhân đã đăng ký. Giá trị hiện tại: ${formatDate(data.ngaySinh)}`);
                }
                
                if (currentGioiTinh && data.gioiTinh && currentGioiTinh !== data.gioiTinh) {
                    showCrossWarningNguoiDaiDien('GioiTinh', 
                        `Giới tính phải giống với thông tin khách hàng cá nhân đã đăng ký. Giá trị hiện tại: ${data.gioiTinh}`);
                }
                
                // Nếu form chưa có data, tự động điền
                if (!currentNguoiDaiDien && data.hoTen) {
                    nguoiDaiDienInput.value = data.hoTen;
                }
                if (!currentNgaySinh && data.ngaySinh) {
                    ngaySinhInput.value = data.ngaySinh;
                }
                if (!currentGioiTinh && data.gioiTinh) {
                    gioiTinhInput.value = data.gioiTinh;
                }
                
                // Lưu thông tin chuẩn để so sánh sau này
                window.cicNguoiDaiDienData = {
                    hoTen: data.hoTen,
                    ngaySinh: data.ngaySinh,
                    gioiTinh: data.gioiTinh
                };
                
                // Cross-validate với CIC
                await checkCrossValidationCIC();
            } else {
                // CCCD chưa có trong hệ thống
                window.cicNguoiDaiDienData = null;
            }
        } catch (error) {
            console.error('Error checking CCCD:', error);
        }
    }
    
    // Hàm format date
    function formatDate(dateStr) {
        if (!dateStr) return '';
        const date = new Date(dateStr);
        return date.toLocaleDateString('vi-VN');
    }
    
    // Hàm hiển thị warning cho người đại diện - HIỂN THỊ NGAY
    function showCrossWarningNguoiDaiDien(fieldName, message) {
        let targetInput = null;
        if (fieldName === 'NguoiDaiDienPhapLuat') {
            targetInput = nguoiDaiDienInput;
        } else if (fieldName === 'NgaySinh') {
            targetInput = ngaySinhInput;
        } else if (fieldName === 'GioiTinh') {
            targetInput = gioiTinhInput;
        }
        
        if (!targetInput) return;
        
        let warningLabel = targetInput.parentElement.querySelector('.cross-validation-warning');
        if (!warningLabel) {
            warningLabel = document.createElement('span');
            warningLabel.className = 'cross-validation-warning text-danger';
            warningLabel.style.fontSize = '0.875rem';
            warningLabel.style.display = 'block';
            warningLabel.style.marginTop = '4px';
            targetInput.parentElement.appendChild(warningLabel);
        }
        
        warningLabel.textContent = message;
        warningLabel.style.display = 'block';
    }
    
    // Thêm listener cho các field người đại diện để kiểm tra NGAY khi thay đổi
    if (nguoiDaiDienInput) {
        nguoiDaiDienInput.addEventListener('input', function() {
            // Nếu đã có data CCCD chuẩn, so sánh ngay
            if (window.cicNguoiDaiDienData && window.cicNguoiDaiDienData.hoTen) {
                const currentValue = this.value.trim();
                const warningLabel = this.parentElement.querySelector('.cross-validation-warning');
                
                if (currentValue && currentValue.toLowerCase() !== window.cicNguoiDaiDienData.hoTen.toLowerCase()) {
                    showCrossWarningNguoiDaiDien('NguoiDaiDienPhapLuat', 
                        `Người đại diện pháp luật phải giống với thông tin khách hàng cá nhân đã đăng ký với CCCD này. Giá trị hiện tại: ${window.cicNguoiDaiDienData.hoTen}`);
                } else if (warningLabel) {
                    warningLabel.style.display = 'none';
                }
            }
        });
    }
    
    if (ngaySinhInput) {
        ngaySinhInput.addEventListener('change', function() {
            if (window.cicNguoiDaiDienData && window.cicNguoiDaiDienData.ngaySinh) {
                const currentValue = this.value;
                const warningLabel = this.parentElement.querySelector('.cross-validation-warning');
                
                if (currentValue && currentValue !== window.cicNguoiDaiDienData.ngaySinh) {
                    showCrossWarningNguoiDaiDien('NgaySinh', 
                        `Ngày sinh phải giống với thông tin khách hàng cá nhân đã đăng ký. Giá trị hiện tại: ${formatDate(window.cicNguoiDaiDienData.ngaySinh)}`);
                } else if (warningLabel) {
                    warningLabel.style.display = 'none';
                }
            }
        });
    }
    
    if (gioiTinhInput) {
        gioiTinhInput.addEventListener('change', function() {
            if (window.cicNguoiDaiDienData && window.cicNguoiDaiDienData.gioiTinh) {
                const currentValue = this.value;
                const warningLabel = this.parentElement.querySelector('.cross-validation-warning');
                
                if (currentValue && currentValue !== window.cicNguoiDaiDienData.gioiTinh) {
                    showCrossWarningNguoiDaiDien('GioiTinh', 
                        `Giới tính phải giống với thông tin khách hàng cá nhân đã đăng ký. Giá trị hiện tại: ${window.cicNguoiDaiDienData.gioiTinh}`);
                } else if (warningLabel) {
                    warningLabel.style.display = 'none';
                }
            }
        });
    }

    
    // Hàm kiểm tra cross-validation với CIC
    async function checkCrossValidationWithCIC() {
        const soCccd = cccdInput?.value?.trim() || '';
        const nguoiDaiDien = nguoiDaiDienInput?.value?.trim() || '';
        const ngaySinh = ngaySinhInput?.value || '';
        const gioiTinh = gioiTinhInput?.value || '';
        
        if (soCccd.length !== 12) return;
        
        try {
            const response = await fetch(`/Customer/CrossCheckWithCIC?soCccd=${encodeURIComponent(soCccd)}&nguoiDaiDien=${encodeURIComponent(nguoiDaiDien)}&ngaySinh=${encodeURIComponent(ngaySinh)}&gioiTinh=${encodeURIComponent(gioiTinh)}`);
            const data = await response.json();
            
            // Hiển thị cảnh báo nếu thông tin không khớp
            if (data.hasWarnings) {
                data.warnings.forEach(warning => {
                    showCrossWarning(warning.fieldName, warning.message);
                });
            } else {
                hideAllCrossWarnings();
            }
        } catch (error) {
            console.error('Error cross-checking with CIC:', error);
        }
    }
    
    // Hàm hiển thị cảnh báo cross-validation
    function showCrossWarning(fieldName, message) {
        let targetInput = null;
        if (fieldName === 'NguoiDaiDienPhapLuat') {
            targetInput = nguoiDaiDienInput;
        } else if (fieldName === 'NgaySinh') {
            targetInput = ngaySinhInput;
        } else if (fieldName === 'GioiTinh') {
            targetInput = gioiTinhInput;
        }
        
        if (!targetInput) return;
        
        let warningLabel = targetInput.parentElement.querySelector('.cross-validation-warning');
        if (!warningLabel) {
            warningLabel = document.createElement('div');
            warningLabel.className = 'cross-validation-warning text-danger';
            warningLabel.style.cssText = 'margin-top: 0.5rem; padding: 0.75rem; background: #fee2e2; border: 1px solid #ef4444; border-radius: 6px; font-size: 0.875rem;';
            targetInput.parentElement.appendChild(warningLabel);
        }
        
        warningLabel.innerHTML = `<strong>⚠ Cảnh báo:</strong> ${message}`;
        warningLabel.style.display = 'block';
    }
    
    // Hàm ẩn cảnh báo
    function hideAllCrossWarnings() {
        const warnings = document.querySelectorAll('.cross-validation-warning');
        warnings.forEach(w => w.style.display = 'none');
    }

    // Validate số lượng nhân viên không được âm
    const soLuongNhanVien = document.getElementById('soLuongNhanVien');
    if (soLuongNhanVien) {
        soLuongNhanVien.addEventListener('input', function(e) {
            let value = parseFloat(e.target.value);
            const errorSpan = this.parentElement.querySelector('.validation-error');
            
            if (isNaN(value) || value < 0) {
                e.target.value = '';
                if (errorSpan) {
                    errorSpan.textContent = 'Số lượng nhân viên không được âm.';
                    errorSpan.style.display = 'block';
                }
            } else {
                // Đảm bảo là số nguyên
                e.target.value = Math.floor(value);
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

    // =============================================
    // CIC VALIDATION REALTIME - DOANH NGHIỆP
    // =============================================
    const tenCongTyCicInput = document.querySelector('[name="TenCongTy"]');
    const cccdNguoiDaiDienInput = document.querySelector('[name="SoCccdNguoiDaiDienPhapLuat"]');
    let cicValidationTimeout = null;

    // Hàm hiển thị CIC warning label
    function showCicWarning(fieldName, message) {
        let targetInput = null;
        if (fieldName === 'TenCongTy') {
            targetInput = tenCongTyCicInput;
        } else if (fieldName === 'SoCccdNguoiDaiDienPhapLuat') {
            targetInput = cccdNguoiDaiDienInput;
        } else if (fieldName === 'MaSoThue') {
            targetInput = maSoThueInput;
        }

        if (!targetInput) return;

        // Tìm hoặc tạo warning label
        let warningLabel = targetInput.parentElement.querySelector('.cic-warning-label');
        if (!warningLabel) {
            warningLabel = document.createElement('div');
            warningLabel.className = 'cic-warning-label';
            warningLabel.style.cssText = 'background: #fef3c7; border: 1px solid #f59e0b; color: #92400e; padding: 8px 12px; border-radius: 6px; margin-top: 8px; font-size: 13px; display: flex; align-items: flex-start; gap: 8px;';
            targetInput.parentElement.appendChild(warningLabel);
        }

        warningLabel.innerHTML = `
            <svg style="width: 16px; height: 16px; flex-shrink: 0; margin-top: 2px;" viewBox="0 0 24 24" fill="none">
                <path d="M12 9V13M12 17H12.01M4.93 19H19.07C20.77 19 21.77 17.17 20.92 15.75L13.85 3.63C13 2.21 11 2.21 10.15 3.63L3.08 15.75C2.23 17.17 3.23 19 4.93 19Z" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
            </svg>
            <div>
                <strong style="display: block; margin-bottom: 2px;">⚠ Cảnh báo từ CIC</strong>
                <span>${message}</span>
            </div>
        `;
        warningLabel.style.display = 'flex';
    }

    // Hàm ẩn CIC warning label
    function hideCicWarning(fieldName) {
        let targetInput = null;
        if (fieldName === 'TenCongTy') {
            targetInput = tenCongTyCicInput;
        } else if (fieldName === 'SoCccdNguoiDaiDienPhapLuat') {
            targetInput = cccdNguoiDaiDienInput;
        } else if (fieldName === 'MaSoThue') {
            targetInput = maSoThueInput;
        }

        if (!targetInput) return;

        const warningLabel = targetInput.parentElement.querySelector('.cic-warning-label');
        if (warningLabel) {
            warningLabel.style.display = 'none';
        }
    }

    // Hàm ẩn tất cả CIC warning
    function hideAllCicWarnings() {
        hideCicWarning('TenCongTy');
        hideCicWarning('SoCccdNguoiDaiDienPhapLuat');
        hideCicWarning('MaSoThue');
    }

    // Hàm kiểm tra CIC cho doanh nghiệp
    async function checkCicValidationDoanhNghiep() {
        const maSoThue = maSoThueHidden?.value || '';
        const tenCongTy = tenCongTyCicInput?.value?.trim() || '';
        const soCccdNguoiDaiDien = cccdNguoiDaiDienInput?.value?.trim() || '';

        // Chỉ kiểm tra khi MST đủ 13 số
        if (maSoThue.length !== 13) {
            hideAllCicWarnings();
            return;
        }

        try {
            const checkCicDoanhNghiepUrl = window.location.origin + '/Customer/CheckCicForDoanhNghiep';
            const response = await fetch(`${checkCicDoanhNghiepUrl}?maSoThue=${encodeURIComponent(maSoThue)}&soCccdNguoiDaiDien=${encodeURIComponent(soCccdNguoiDaiDien)}&tenCongTy=${encodeURIComponent(tenCongTy)}`);
            const data = await response.json();

            if (data.hasCic && !data.isValid) {
                showCicWarning(data.fieldName, data.errorMessage);
            } else {
                hideAllCicWarnings();
            }
        } catch (error) {
            console.error('Error checking CIC for Doanh nghiệp:', error);
        }
    }

    // Debounce function
    function debounceCic(func, delay) {
        return function(...args) {
            if (cicValidationTimeout) clearTimeout(cicValidationTimeout);
            cicValidationTimeout = setTimeout(() => func.apply(this, args), delay);
        };
    }

    // Gắn sự kiện cho các input
    if (maSoThueInput && tenCongTyCicInput && cccdNguoiDaiDienInput) {
        const debouncedCheckCic = debounceCic(checkCicValidationDoanhNghiep, 500);
        
        // Khi MST thay đổi
        maSoThueInput.addEventListener('input', debouncedCheckCic);
        maSoThueInput.addEventListener('blur', checkCicValidationDoanhNghiep);
        
        // Khi Tên công ty thay đổi
        tenCongTyCicInput.addEventListener('input', debouncedCheckCic);
        tenCongTyCicInput.addEventListener('blur', checkCicValidationDoanhNghiep);
        
        // Khi CCCD người đại diện thay đổi
        cccdNguoiDaiDienInput.addEventListener('input', debouncedCheckCic);
        cccdNguoiDaiDienInput.addEventListener('blur', checkCicValidationDoanhNghiep);
    }
    // =============================================
    // END CIC VALIDATION
    // =============================================

    // =============================================
    // CROSS-VALIDATION CCCD - KIỂM TRA VỚI CÁ NHÂN/CIC CÁ NHÂN
    // =============================================
    const nguoiDaiDienInput = document.querySelector('[name="NguoiDaiDienPhapLuat"]');
    const ngaySinhNguoiDaiDienInput = document.querySelector('[name="NgaySinh"]');
    const gioiTinhNguoiDaiDienInput = document.querySelector('[name="GioiTinh"]');
    let crossValidationTimeout = null;

    // Hàm hiển thị Cross warning label cho doanh nghiệp
    function showCrossWarningDN(fieldName, message) {
        let targetInput = null;
        if (fieldName === 'NguoiDaiDienPhapLuat') {
            targetInput = nguoiDaiDienInput;
        } else if (fieldName === 'NgaySinh') {
            targetInput = ngaySinhNguoiDaiDienInput;
        } else if (fieldName === 'GioiTinh') {
            targetInput = gioiTinhNguoiDaiDienInput;
        }

        if (!targetInput) return;

        // Tìm hoặc tạo warning label
        let warningLabel = targetInput.parentElement.querySelector('.cross-warning-label');
        if (!warningLabel) {
            warningLabel = document.createElement('div');
            warningLabel.className = 'cross-warning-label';
            warningLabel.style.cssText = 'background: #fee2e2; border: 1px solid #ef4444; color: #991b1b; padding: 8px 12px; border-radius: 6px; margin-top: 8px; font-size: 13px; display: flex; align-items: flex-start; gap: 8px;';
            targetInput.parentElement.appendChild(warningLabel);
        }

        warningLabel.innerHTML = `
            <svg style="width: 16px; height: 16px; flex-shrink: 0; margin-top: 2px;" viewBox="0 0 24 24" fill="none">
                <path d="M12 9V13M12 17H12.01M4.93 19H19.07C20.77 19 21.77 17.17 20.92 15.75L13.85 3.63C13 2.21 11 2.21 10.15 3.63L3.08 15.75C2.23 17.17 3.23 19 4.93 19Z" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
            </svg>
            <div>
                <strong style="display: block; margin-bottom: 2px;">⚠ Không khớp với Khách hàng cá nhân/CIC</strong>
                <span>${message}</span>
            </div>
        `;
        warningLabel.style.display = 'flex';
    }

    // Hàm ẩn Cross warning label
    function hideCrossWarningDN(fieldName) {
        let targetInput = null;
        if (fieldName === 'NguoiDaiDienPhapLuat') {
            targetInput = nguoiDaiDienInput;
        } else if (fieldName === 'NgaySinh') {
            targetInput = ngaySinhNguoiDaiDienInput;
        } else if (fieldName === 'GioiTinh') {
            targetInput = gioiTinhNguoiDaiDienInput;
        }

        if (!targetInput) return;

        const warningLabel = targetInput.parentElement.querySelector('.cross-warning-label');
        if (warningLabel) {
            warningLabel.style.display = 'none';
        }
    }

    // Hàm ẩn tất cả cross warnings
    function hideAllCrossWarningsDN() {
        hideCrossWarningDN('NguoiDaiDienPhapLuat');
        hideCrossWarningDN('NgaySinh');
        hideCrossWarningDN('GioiTinh');
    }

    // Hàm kiểm tra Cross-validation cho doanh nghiệp
    async function checkCrossValidationDoanhNghiep() {
        const soCccd = cccdNguoiDaiDienInput?.value?.trim() || '';
        const nguoiDaiDien = nguoiDaiDienInput?.value?.trim() || '';
        const ngaySinh = ngaySinhNguoiDaiDienInput?.value || '';
        const gioiTinh = gioiTinhNguoiDaiDienInput?.value || '';

        // Chỉ kiểm tra khi CCCD đủ 12 số
        if (soCccd.length !== 12) {
            hideAllCrossWarningsDN();
            return;
        }

        try {
            const crossCheckUrl = window.location.origin + '/Customer/CrossCheckCccdForDoanhNghiep';
            const response = await fetch(`${crossCheckUrl}?soCccd=${encodeURIComponent(soCccd)}&nguoiDaiDien=${encodeURIComponent(nguoiDaiDien)}&ngaySinh=${encodeURIComponent(ngaySinh)}&gioiTinh=${encodeURIComponent(gioiTinh)}`);
            const data = await response.json();

            if (data.hasExistingData && !data.isValid && data.errors) {
                hideAllCrossWarningsDN();
                data.errors.forEach(err => {
                    showCrossWarningDN(err.fieldName, err.errorMessage);
                });
            } else {
                hideAllCrossWarningsDN();
            }
        } catch (error) {
            console.error('Error cross-checking CCCD for Doanh nghiệp:', error);
        }
    }

    // Debounce function for cross validation
    function debounceCrossValidation(func, delay) {
        return function(...args) {
            if (crossValidationTimeout) clearTimeout(crossValidationTimeout);
            crossValidationTimeout = setTimeout(() => func.apply(this, args), delay);
        };
    }

    // Gắn sự kiện cho các field người đại diện
    if (cccdNguoiDaiDienInput && nguoiDaiDienInput && ngaySinhNguoiDaiDienInput && gioiTinhNguoiDaiDienInput) {
        const debouncedCrossCheck = debounceCrossValidation(checkCrossValidationDoanhNghiep, 500);
        
        // Khi CCCD người đại diện thay đổi
        cccdNguoiDaiDienInput.addEventListener('input', debouncedCrossCheck);
        cccdNguoiDaiDienInput.addEventListener('blur', checkCrossValidationDoanhNghiep);
        
        // Khi Người đại diện pháp luật thay đổi
        nguoiDaiDienInput.addEventListener('input', debouncedCrossCheck);
        nguoiDaiDienInput.addEventListener('blur', checkCrossValidationDoanhNghiep);
        
        // Khi Ngày sinh thay đổi
        ngaySinhNguoiDaiDienInput.addEventListener('change', checkCrossValidationDoanhNghiep);
        
        // Khi Giới tính thay đổi
        gioiTinhNguoiDaiDienInput.addEventListener('change', checkCrossValidationDoanhNghiep);
    }
    // =============================================
    // END CROSS-VALIDATION
    // =============================================

    // Form submit validation - hiển thị TẤT CẢ lỗi khi submit
    const form = document.querySelector('form.customer-form');
    if (form) {
        form.addEventListener('submit', function(e) {
            let hasError = false;
            
            // Validate TẤT CẢ required fields
            const requiredInputs = form.querySelectorAll('input[required], select[required], textarea[required]');
            requiredInputs.forEach(input => {
                const value = input.value.trim();
                const errorSpan = input.parentElement.querySelector('.validation-error') || 
                                input.nextElementSibling;
                const fieldName = input.parentElement.querySelector('label')?.textContent.replace('*', '').trim() || 'Trường này';
                
                if (!value || value === '') {
                    if (errorSpan) {
                        errorSpan.textContent = fieldName + ' là bắt buộc.';
                        errorSpan.style.display = 'block';
                    }
                    hasError = true;
                    
                    // Focus vào field đầu tiên có lỗi
                    if (hasError && !form.querySelector('input:focus, select:focus, textarea:focus')) {
                        input.focus();
                    }
                }
            });
            
            // Validate số điện thoại
            if (soDienThoaiInput) {
                const phone = soDienThoaiInput.value.replace(/\D/g, '');
                const errorSpan = soDienThoaiInput.parentElement.querySelector('.validation-error');
                if (phone && phone.length !== 10) {
                    if (errorSpan) {
                        errorSpan.textContent = 'Số điện thoại phải đủ 10 chữ số.';
                        errorSpan.style.display = 'block';
                    }
                    hasError = true;
                }
            }
            
            // Validate email format
            if (emailInput) {
                const email = emailInput.value.trim();
                const errorSpan = emailInput.parentElement.querySelector('.validation-error');
                if (email) {
                    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
                    if (!emailRegex.test(email)) {
                        if (errorSpan) {
                            errorSpan.textContent = 'Email không đúng định dạng.';
                            errorSpan.style.display = 'block';
                        }
                        hasError = true;
                    }
                }
            }
            
            // Validate CCCD
            const cccdInput = form.querySelector('input[name="SoCccdNguoiDaiDienPhapLuat"]');
            if (cccdInput) {
                const cccd = cccdInput.value.replace(/\D/g, '');
                const errorSpan = cccdInput.parentElement.querySelector('.validation-error');
                if (cccd && cccd.length !== 12) {
                    if (errorSpan) {
                        errorSpan.textContent = 'Số CCCD phải đủ 12 chữ số.';
                        errorSpan.style.display = 'block';
                    }
                    hasError = true;
                }
            }
            
            // Validate Mã số thuế
            if (maSoThueHidden) {
                const mst = maSoThueHidden.value;
                const errorSpan = document.getElementById('maSoThueError');
                if (mst && mst.length !== 13) {
                    if (errorSpan) {
                        errorSpan.textContent = 'Mã số thuế phải đủ 13 chữ số.';
                        errorSpan.style.display = 'block';
                    }
                    hasError = true;
                }
            }
            
            if (hasError) {
                e.preventDefault();
                alert('Vui lòng kiểm tra lại các trường thông tin được đánh dấu đỏ.');
                return false;
            }
        });
    }
