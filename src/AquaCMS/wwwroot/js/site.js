/**
 * site.js — Client-side logic cho CatalogaWeb.
 * - Giỏ hàng (localStorage based)
 * - Toast notifications
 * - Utility functions
 */

// ==========================================
// TOAST NOTIFICATIONS
// ==========================================
var Toast = {
    /**
     * Hiển thị toast notification.
     * @param {string} message - Nội dung thông báo
     * @param {'success'|'error'|'info'} type - Loại toast
     * @param {number} duration - Thời gian hiển thị (ms)
     */
    show: function(message, type, duration) {
        type = type || 'info';
        duration = duration || 3000;

        var toast = document.createElement('div');
        toast.className = 'toast toast-' + type;
        toast.textContent = message;
        document.body.appendChild(toast);

        // Trigger animation
        requestAnimationFrame(function() {
            toast.classList.add('show');
        });

        // Auto remove
        setTimeout(function() {
            toast.classList.remove('show');
            setTimeout(function() { toast.remove(); }, 300);
        }, duration);
    }
};

// ==========================================
// GIỎ HÀNG (localStorage)
// ==========================================
var Cart = {
    STORAGE_KEY: 'catalogaweb_cart',

    /** Lấy danh sách item trong giỏ */
    getItems: function() {
        try {
            return JSON.parse(localStorage.getItem(this.STORAGE_KEY)) || [];
        } catch (e) {
            return [];
        }
    },

    /** Lưu giỏ hàng */
    save: function(items) {
        localStorage.setItem(this.STORAGE_KEY, JSON.stringify(items));
        this.updateBadge();
    },

    /** Thêm sản phẩm vào giỏ */
    add: function(product) {
        var items = this.getItems();
        var existing = items.find(function(item) { return item.id === product.id; });

        if (existing) {
            existing.quantity += 1;
        } else {
            items.push({
                id: product.id,
                name: product.name,
                price: product.price,
                image: product.image,
                quantity: 1
            });
        }

        this.save(items);
        Toast.show('Đã thêm vào giỏ hàng!', 'success');
    },

    /** Xóa item khỏi giỏ */
    remove: function(productId) {
        var items = this.getItems().filter(function(item) { return item.id !== productId; });
        this.save(items);
    },

    /** Cập nhật số lượng */
    updateQuantity: function(productId, quantity) {
        var items = this.getItems();
        var item = items.find(function(i) { return i.id === productId; });
        if (item) {
            item.quantity = Math.max(1, quantity);
            this.save(items);
        }
    },

    /** Xóa toàn bộ giỏ */
    clear: function() {
        localStorage.removeItem(this.STORAGE_KEY);
        this.updateBadge();
    },

    /** Tổng số item */
    getTotalCount: function() {
        return this.getItems().reduce(function(sum, item) { return sum + item.quantity; }, 0);
    },

    /** Cập nhật badge trên navbar */
    updateBadge: function() {
        var badge = document.getElementById('cart-badge');
        if (!badge) return;

        var count = this.getTotalCount();
        if (count > 0) {
            badge.textContent = count > 99 ? '99+' : count;
            badge.classList.remove('hidden');
            badge.classList.add('flex');
        } else {
            badge.classList.add('hidden');
            badge.classList.remove('flex');
        }
    }
};

// ==========================================
// INIT
// ==========================================
document.addEventListener('DOMContentLoaded', function() {
    // Cập nhật cart badge khi page load
    Cart.updateBadge();

    // Event delegation cho nút "Thêm vào giỏ hàng"
    document.addEventListener('click', function(e) {
        var btn = e.target.closest('.add-to-cart-btn');
        if (!btn) return;

        e.preventDefault();
        Cart.add({
            id: btn.dataset.productId,
            name: btn.dataset.productName,
            price: btn.dataset.productPrice ? parseFloat(btn.dataset.productPrice) : null,
            image: btn.dataset.productImage || ''
        });
    });
});
