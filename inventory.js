// Sample inventory data
const sampleInventory = [
  {
    id: 1,
    name: "Oxytocin",
    category: "Medicine",
    quantity: 80,
    unit: "vial",
    reorderLevel: 20,
    expirationDate: "2026-12-31",
    supplier: "PhilMed Supplies Inc.",
    dateAdded: "2026-01-15",
    lastUpdated: "2026-03-20"
  },
  {
    id: 2,
    name: "Gauze Pads",
    category: "Supplies",
    quantity: 5,
    unit: "pack",
    reorderLevel: 15,
    expirationDate: "2027-06-30",
    supplier: "MedChoice PH",
    dateAdded: "2026-01-15",
    lastUpdated: "2026-03-22"
  },
  {
    id: 3,
    name: "Surgical Gloves",
    category: "Supplies",
    quantity: 8,
    unit: "box",
    reorderLevel: 10,
    expirationDate: "2027-03-15",
    supplier: "MedChoice PH",
    dateAdded: "2026-01-15",
    lastUpdated: "2026-03-22"
  },
  {
    id: 4,
    name: "IV Fluids",
    category: "Medicine",
    quantity: 45,
    unit: "bag",
    reorderLevel: 15,
    expirationDate: "2026-09-30",
    supplier: "PhilMed Supplies Inc.",
    dateAdded: "2026-01-15",
    lastUpdated: "2026-03-20"
  },
  {
    id: 5,
    name: "Delivery Kit",
    category: "Equipment",
    quantity: 0,
    unit: "set",
    reorderLevel: 2,
    expirationDate: "2028-01-01",
    supplier: "MediTech Solutions",
    dateAdded: "2026-02-10",
    lastUpdated: "2026-03-18"
  },
  {
    id: 6,
    name: "Sterilization Unit",
    category: "Equipment",
    quantity: 3,
    unit: "unit",
    reorderLevel: 1,
    expirationDate: "2030-06-15",
    supplier: "PhilMed Supplies Inc.",
    dateAdded: "2025-11-20",
    lastUpdated: "2026-03-10"
  }
];

// Initialize localStorage if empty
function initializeInventory() {
  if (!localStorage.getItem('inventoryItems')) {
    localStorage.setItem('inventoryItems', JSON.stringify(sampleInventory));
  }
}

// Get item status based on quantity and expiration date
function getItemStatus(quantity, reorderLevel, expirationDate) {
  const today = new Date();
  const expDate = new Date(expirationDate);
  const daysUntilExpiry = Math.ceil((expDate - today) / (1000 * 60 * 60 * 24));

  if (quantity === 0) return 'Out of Stock';
  if (quantity <= reorderLevel) return 'Low Stock';
  if (daysUntilExpiry <= 30 && daysUntilExpiry >= 0) return 'Expiring Soon';
  return 'Good Stock';
}

// Format date
function formatDate(dateString) {
  const options = { year: 'numeric', month: 'short', day: 'numeric' };
  return new Date(dateString).toLocaleDateString('en-US', options);
}

// Get category icon
function getCategoryIcon(category) {
  const icons = {
    'Medicine': 'bi-capsule',
    'Equipment': 'bi-tools',
    'Supplies': 'bi-box-seam'
  };
  return icons[category] || 'bi-box-seam';
}

// Get category class
function getCategoryClass(category) {
  return category.toLowerCase().replace(' ', '-');
}

// Show toast notification
function showToast(message, type = 'success') {
  const toast = document.createElement('div');
  toast.className = `toast toast-${type}`;
  toast.innerHTML = `
    <i class="bi bi-check-circle-fill" aria-hidden="true"></i>
    <span>${message}</span>
  `;
  document.body.appendChild(toast);

  setTimeout(() => {
    toast.classList.add('fade-out');
    setTimeout(() => toast.remove(), 300);
  }, 3000);
}

// Check and display low stock alert
function updateLowStockAlert(items) {
  const lowItems = items.filter(item =>
    item.quantity <= item.reorderLevel || item.quantity === 0
  );

  const banner = document.getElementById('low-stock-banner');

  if (lowItems.length > 0) {
    const names = lowItems.map(i => i.name).join(', ');
    const message = document.getElementById('alert-message');
    message.textContent = `Low stock alert! The following items need restocking: ${names}. Please update your inventory.`;
    banner.classList.remove('is-hidden');
  } else {
    banner.classList.add('is-hidden');
  }
}

// Calculate stats
function updateStats(items) {
  const today = new Date();
  let totalItems = items.length;
  let lowStockCount = 0;
  let expiringCount = 0;
  let outOfStockCount = 0;

  items.forEach(item => {
    if (item.quantity === 0) {
      outOfStockCount++;
      lowStockCount++;
    } else if (item.quantity <= item.reorderLevel) {
      lowStockCount++;
    }

    const expDate = new Date(item.expirationDate);
    const daysUntilExpiry = Math.ceil((expDate - today) / (1000 * 60 * 60 * 24));
    if (daysUntilExpiry <= 30 && daysUntilExpiry >= 0) {
      expiringCount++;
    }
  });

  document.getElementById('stat-total').textContent = totalItems;
  document.getElementById('stat-low-stock').textContent = lowStockCount;
  document.getElementById('stat-expiring').textContent = expiringCount;
  document.getElementById('stat-out-of-stock').textContent = outOfStockCount;
}

// Render table
function renderTable(items) {
  const tableBody = document.getElementById('tableBody');
  const emptyState = document.getElementById('emptyState');

  if (items.length === 0) {
    tableBody.innerHTML = '';
    emptyState.classList.remove('is-hidden');
    return;
  }

  emptyState.classList.add('is-hidden');
  tableBody.innerHTML = items.map((item, index) => {
    const status = getItemStatus(item.quantity, item.reorderLevel, item.expirationDate);
    const statusClass = status
      .toLowerCase()
      .replace(' ', '-')
      .replace('out-of-stock', 'out');

    const expDate = new Date(item.expirationDate);
    const today = new Date();
    const daysUntilExpiry = Math.ceil((expDate - today) / (1000 * 60 * 60 * 24));

    let expiryDisplay = formatDate(item.expirationDate);
    let expiryIcon = '';

    if (daysUntilExpiry < 0) {
      expiryDisplay = `<span class="expiry-danger"><i class="bi bi-exclamation-circle-fill" aria-hidden="true"></i>${expiryDisplay}</span>`;
    } else if (daysUntilExpiry <= 30) {
      expiryDisplay = `<span class="expiry-warning"><i class="bi bi-calendar-event" aria-hidden="true"></i>${expiryDisplay}</span>`;
    }

    // Calculate progress bar
    const maxQty = Math.max(item.reorderLevel * 3, item.quantity);
    const percentage = (item.quantity / maxQty) * 100;
    let barClass = 'good';
    if (item.quantity === 0) barClass = 'critical';
    else if (item.quantity <= item.reorderLevel) barClass = 'low';

    return `
      <tr>
        <td>${index + 1}</td>
        <td>
          <div class="item-name-cell">
            <div class="category-icon ${getCategoryClass(item.category)}" aria-hidden="true">
              <i class="bi ${getCategoryIcon(item.category)}"></i>
            </div>
            <span class="item-name">${item.name}</span>
          </div>
        </td>
        <td>${item.category}</td>
        <td>
          <div class="quantity-cell">
            <div class="qty-display">${item.quantity} ${item.unit}</div>
            <div class="qty-bar">
              <div class="qty-bar-fill ${barClass}" style="width: ${Math.min(percentage, 100)}%"></div>
            </div>
          </div>
        </td>
        <td class="reorder-level">${item.reorderLevel} ${item.unit}</td>
        <td>${expiryDisplay}</td>
        <td>${item.supplier}</td>
        <td>${item.lastUpdated}</td>
        <td>
          <span class="status-badge status-${statusClass}">${status}</span>
        </td>
        <td>
          <div class="actions-cell">
            <button class="btn-action" title="Quick update" onclick="openQuickUpdate(${item.id}, '${item.name}', ${item.quantity}, '${item.unit}')">
              <i class="bi bi-plus-square" aria-hidden="true"></i>
            </button>
            <button class="btn-action" title="Edit" onclick="window.location.href='inventory-edit.html?id=${item.id}'">
              <i class="bi bi-pencil" aria-hidden="true"></i>
            </button>
            <button class="btn-action btn-action-delete" title="Delete" onclick="openDeleteModal(${item.id}, '${item.name}')">
              <i class="bi bi-trash" aria-hidden="true"></i>
            </button>
          </div>
        </td>
      </tr>
    `;
  }).join('');
}

// Load and display inventory
function loadInventory() {
  const items = JSON.parse(localStorage.getItem('inventoryItems')) || [];
  updateStats(items);
  updateLowStockAlert(items);
  renderTable(items);
}

// Filter and sort logic
function applyFiltersAndSort() {
  const items = JSON.parse(localStorage.getItem('inventoryItems')) || [];
  const searchTerm = document.getElementById('searchInput').value.toLowerCase();
  const categoryFilter = document.getElementById('categoryFilter').value;
  const statusFilter = document.getElementById('statusFilter').value;
  const sortBy = document.getElementById('sortBy').value;

  let filtered = items.filter(item => {
    const matchesSearch = item.name.toLowerCase().includes(searchTerm) ||
                         item.supplier.toLowerCase().includes(searchTerm);
    const matchesCategory = !categoryFilter || item.category === categoryFilter;
    const matchesStatus = !statusFilter || getItemStatus(item.quantity, item.reorderLevel, item.expirationDate) === statusFilter;

    return matchesSearch && matchesCategory && matchesStatus;
  });

  // Sort
  sorted = [...filtered].sort((a, b) => {
    switch (sortBy) {
      case 'name-asc':
        return a.name.localeCompare(b.name);
      case 'name-desc':
        return b.name.localeCompare(a.name);
      case 'qty-asc':
        return a.quantity - b.quantity;
      case 'qty-desc':
        return b.quantity - a.quantity;
      case 'expiry-asc':
        return new Date(a.expirationDate) - new Date(b.expirationDate);
      default:
        return 0;
    }
  });

  renderTable(sorted);
}

// Quick Update Modal
let currentItemId = null;

function openQuickUpdate(itemId, itemName, currentQty, unit) {
  currentItemId = itemId;
  document.getElementById('modalItemName').textContent = itemName;
  document.getElementById('modalCurrentQty').textContent = currentQty + ' ' + unit;
  document.getElementById('modalUnit').textContent = unit;
  document.getElementById('modalNewQty').value = '';
  document.getElementById('quickUpdateModal').classList.remove('is-hidden');
  document.getElementById('quickUpdateModal').setAttribute('aria-hidden', 'false');
  document.getElementById('modalNewQty').focus();
}

function closeQuickUpdate() {
  document.getElementById('quickUpdateModal').classList.add('is-hidden');
  document.getElementById('quickUpdateModal').setAttribute('aria-hidden', 'true');
  currentItemId = null;
}

function saveQuickUpdate() {
  const newQty = parseInt(document.getElementById('modalNewQty').value);

  if (isNaN(newQty) || newQty < 0) {
    alert('Please enter a valid quantity.');
    return;
  }

  let items = JSON.parse(localStorage.getItem('inventoryItems')) || [];
  items = items.map(item => {
    if (item.id === currentItemId) {
      item.quantity = newQty;
      item.lastUpdated = new Date().toLocaleDateString('en-PH');
    }
    return item;
  });

  localStorage.setItem('inventoryItems', JSON.stringify(items));
  closeQuickUpdate();
  loadInventory();
  applyFiltersAndSort();
  showToast('Stock updated successfully!');
}

// Delete Modal
let itemToDelete = { id: null, name: '' };

function openDeleteModal(itemId, itemName) {
  itemToDelete = { id: itemId, name: itemName };
  document.getElementById('deleteItemName').textContent = itemName;
  document.getElementById('deleteModal').classList.remove('is-hidden');
  document.getElementById('deleteModal').setAttribute('aria-hidden', 'false');
}

function closeDeleteModal() {
  document.getElementById('deleteModal').classList.add('is-hidden');
  document.getElementById('deleteModal').setAttribute('aria-hidden', 'true');
  itemToDelete = { id: null, name: '' };
}

function confirmDelete() {
  if (!itemToDelete.id) return;

  let items = JSON.parse(localStorage.getItem('inventoryItems')) || [];
  items = items.filter(item => item.id !== itemToDelete.id);
  localStorage.setItem('inventoryItems', JSON.stringify(items));

  closeDeleteModal();
  loadInventory();
  applyFiltersAndSort();
  showToast('Item deleted successfully!');
}

// Close modals on overlay click
document.addEventListener('DOMContentLoaded', function() {
  initializeInventory();
  loadInventory();

  // Modal overlay clicks
  document.getElementById('quickUpdateModal').addEventListener('click', function(e) {
    if (e.target === this) closeQuickUpdate();
  });

  document.getElementById('deleteModal').addEventListener('click', function(e) {
    if (e.target === this) closeDeleteModal();
  });

  // Button clicks
  document.getElementById('addNewItemBtn').addEventListener('click', function() {
    window.location.href = 'inventory-add.html';
  });

  document.getElementById('emptyStateAddBtn').addEventListener('click', function() {
    window.location.href = 'inventory-add.html';
  });

  document.getElementById('cancelUpdateBtn').addEventListener('click', closeQuickUpdate);
  document.getElementById('saveUpdateBtn').addEventListener('click', saveQuickUpdate);

  document.getElementById('cancelDeleteBtn').addEventListener('click', closeDeleteModal);
  document.getElementById('confirmDeleteBtn').addEventListener('click', confirmDelete);

  // Search and filter listeners
  document.getElementById('searchInput').addEventListener('input', applyFiltersAndSort);
  document.getElementById('categoryFilter').addEventListener('change', applyFiltersAndSort);
  document.getElementById('statusFilter').addEventListener('change', applyFiltersAndSort);
  document.getElementById('sortBy').addEventListener('change', applyFiltersAndSort);

  // Allow Enter key in quick update
  document.getElementById('modalNewQty').addEventListener('keypress', function(e) {
    if (e.key === 'Enter') saveQuickUpdate();
  });
});
