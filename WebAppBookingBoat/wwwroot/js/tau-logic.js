

document.addEventListener("DOMContentLoaded", function () {
    // Tự động tăng tổng số lượng ghế = ghế thường + ghế vip
    function initAutoSumSeats() {
        const gheThuong = document.getElementById('SoGheThuong');
        const gheVIP = document.getElementById('SoGheVIP');
        const tongGhe = document.getElementById('TongSoGhe');

        if (gheThuong && gheVIP && tongGhe) {
            const calculate = () => {
                const val1 = parseInt(gheThuong.value) || 0;
                const val2 = parseInt(gheVIP.value) || 0;
                tongGhe.value = val1 + val2;
            };

            // Lắng nghe sự kiện khi người dùng nhập số
            gheThuong.addEventListener('input', calculate);
            gheVIP.addEventListener('input', calculate);
        }
    }

    // Logic Preview cho trang Create
    const imageInput = document.getElementById('imageInput');
    const imgPreview = document.getElementById('imgPreview');

    if (imageInput && imgPreview) {
        imageInput.addEventListener('change', function () {
            const file = this.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    imgPreview.src = e.target.result; // Thay đổi đường dẫn ảnh bằng dữ liệu tạm
                };
                reader.readAsDataURL(file);
            }
        });
    }

    // Logic Preview cho trang Edit
    const imageInputEdit = document.getElementById('imageInputEdit');
    const imgPreviewEdit = document.getElementById('imgPreviewEdit');

    if (imageInputEdit && imgPreviewEdit) {
        imageInputEdit.addEventListener('change', function () {
            const file = this.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    imgPreviewEdit.src = e.target.result;
                    // Hiện ảnh preview lên sau khi đã nạp dữ liệu
                    imgPreviewEdit.classList.remove('d-none');
                };
                reader.readAsDataURL(file);
            }
        });
    }
});