/**
 * @file: tau-logic.js
 * @description: Quản lý các tương tác người dùng tại phân hệ Quản lý Tàu thuyền (Admin Area)
 * @author: huymai975
 * @date: 2026-04-10
 */

document.addEventListener("DOMContentLoaded", function () { 
    // --- KHỞI TẠO CÁC TÍNH NĂNG ---
    initAutoSumSeats();
    initImagePreview();
    initQuickToggleStatus();

    /**
     * 1. Tính năng: Tự động tính toán tổng số ghế
     * Cơ chế: Lắng nghe sự kiện nhập liệu tại các trường Ghế Thường và Ghế VIP.
     */
    function initAutoSumSeats() {
        const $gheThuong = $('#SoGheThuong');
        const $gheVIP = $('#SoGheVIP');
        const $tongGhe = $('#TongSoGhe');

        if ($gheThuong.length && $gheVIP.length && $tongGhe.length) {
            const calculate = () => {
                const val1 = parseInt($gheThuong.val()) || 0;
                const val2 = parseInt($gheVIP.val()) || 0;
                $tongGhe.val(val1 + val2);
            };

            $gheThuong.on('input', calculate);
            $gheVIP.on('input', calculate);
        }
    }

    /**
     * 2. Tính năng: Xem trước hình ảnh (Preview Image)
     * Áp dụng cho cả trang khởi tạo (Create) và trang cập nhật (Edit).
     */
    function initImagePreview() {
        // Xử lý cho trang Create (ID: imageInput)
        $(document).on('change', '#imageInput', function () {
            const file = this.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = (e) => $('#imgPreview').attr('src', e.target.result);
                reader.readAsDataURL(file);
            }
        });

        // Xử lý cho trang Edit (ID: imageInputEdit)
        $(document).on('change', '#imageInputEdit', function () {
            const file = this.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = (e) => {
                    const $preview = $('#imgPreviewEdit');
                    $preview.attr('src', e.target.result).removeClass('d-none');
                };
                reader.readAsDataURL(file);
            }
        });
    }

    /**
     * 3. Tính năng: Chuyển đổi trạng thái nhanh (Quick Toggle Status)
     * Cơ chế: Gửi yêu cầu AJAX lên máy chủ để cập nhật trạng thái mà không làm tải lại trang.
     */
    function initQuickToggleStatus() {
        $(document).on('click', '.btn-toggle-status', function (e) {
            e.preventDefault();
            const $btn = $(this);
            const id = $btn.data('id');

            $.post('/Admin/Taus/ToggleStatus', { id: id })
                .done(function (response) {
                    // Kiểm tra logic: Chỉ cần if (response.success) là đủ cân cả thế giới
                    if (response.success) {
                        // 1. Cập nhật giao diện nút
                        updateToggleUI($btn, response.newState);

                        // 2. Thay vì check hàm showNotifications, mình dùng SweetAlert2 cho trang trọng
                        if (typeof Swal !== 'undefined') {
                            const Toast = Swal.mixin({
                                toast: true,
                                position: 'top-end',
                                showConfirmButton: false,
                                timer: 2000,
                                timerProgressBar: true
                            });
                            Toast.fire({
                                icon: 'success',
                                title: 'Cập nhật trạng thái thành công!'
                            });
                        } else {
                            // Nếu chưa cài SweetAlert2 thì dùng tạm console để đỡ bị văng lỗi
                            console.log("Cập nhật thành công!");
                        }
                    } else {
                        // Server báo thành công = false mới hiện cái này
                        alert(response.message || "Không thể cập nhật trạng thái.");
                    }
                })
                .fail(function () {
                    alert("Lỗi kết nối máy chủ!");
                });
        });
    }

    /**
     * Hàm phụ trợ: Cập nhật CSS và Nội dung cho nút Toggle
     */
    function updateToggleUI($btn, isEnabled) {
        const $icon = $btn.find('i');
        const $text = $btn.find('span');

        if (isEnabled) {
            $btn.removeClass('btn-secondary').addClass('btn-success');
            $icon.removeClass('fa-pause-circle').addClass('fa-check-circle');
            $text.text('Đang hoạt động');
        } else {
            $btn.removeClass('btn-success').addClass('btn-secondary');
            $icon.removeClass('fa-check-circle').addClass('fa-pause-circle');
            $text.text('Tạm ngưng');
        }
    }
});