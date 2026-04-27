-- Thêm cột show_price vào bảng products
ALTER TABLE products ADD COLUMN show_price BOOLEAN DEFAULT TRUE;

-- Cập nhật comment cho cột
COMMENT ON COLUMN products.show_price IS 'Có hiển thị giá ra ngoài hay không (TRUE: hiện giá, FALSE: hiện "Liên hệ")';
