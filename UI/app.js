// API base URL
const API_BASE = 'http://localhost:5000';

// DOM elements
const customerTableBody = document.getElementById('customerTableBody');
const customerModal = document.getElementById('customerModal');
const deleteModal = document.getElementById('deleteModal');
const quickEditModal = document.getElementById('quickEditModal');
const customerForm = document.getElementById('customerForm');
const quickEditForm = document.getElementById('quickEditForm');
const modalTitle = document.getElementById('modalTitle');
const addBtn = document.getElementById('addBtn');
const cancelBtn = document.getElementById('cancelBtn');
const cancelDeleteBtn = document.getElementById('cancelDeleteBtn');
const confirmDeleteBtn = document.getElementById('confirmDeleteBtn');
const cancelQuickEditBtn = document.getElementById('cancelQuickEditBtn');

// Current customer being edited or deleted
let currentCustomerId = null;
let currentQuickEditId = null;

// Load customers on page load
document.addEventListener('DOMContentLoaded', loadCustomers);

// Event listeners
addBtn.addEventListener('click', () => openModal());
cancelBtn.addEventListener('click', closeModal);
cancelDeleteBtn.addEventListener('click', closeDeleteModal);
confirmDeleteBtn.addEventListener('click', () => deleteCustomer(currentCustomerId));
customerForm.addEventListener('submit', handleFormSubmit);
cancelQuickEditBtn.addEventListener('click', closeQuickEditModal);
quickEditForm.addEventListener('submit', handleQuickEditSubmit);

// Load all customers
async function loadCustomers() {
    try {
        const response = await fetch(`${API_BASE}/customers`);
        if (!response.ok) throw new Error('Failed to load customers');
        const customers = await response.json();
        displayCustomers(customers);
    } catch (error) {
        console.error('Error loading customers:', error);
        alert('Failed to load customers. Make sure the API is running.');
    }
}

// Display customers in table
function displayCustomers(customers) {
    customerTableBody.innerHTML = '';
    customers.forEach(customer => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">${customer.id}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                ${customer.name}
                <button class="ml-2 text-blue-600 hover:text-blue-900 text-xs edit-name-btn" data-id="${customer.id}" data-name="${customer.name}" title="Quick edit name" type="button">
                    ✏️
                </button>
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">${customer.age}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">${customer.country}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">$${customer.revenue.toFixed(2)}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                <span class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${customer.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}">
                    ${customer.isActive ? 'Active' : 'Inactive'}
                </span>
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">${customer.tags ? customer.tags.join(', ') : ''}</td>
            <td class="px-6 py-4 whitespace-nowrap text-sm font-medium">
                <button class="text-indigo-600 hover:text-indigo-900 mr-2 edit-customer-btn" data-id="${customer.id}" type="button">Edit</button>
                <button class="text-red-600 hover:text-red-900 delete-customer-btn" data-id="${customer.id}" type="button">Delete</button>
            </td>
        `;
        customerTableBody.appendChild(row);
    });

    // Add event listeners for dynamically created buttons
    document.querySelectorAll('.edit-name-btn').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation();
            const id = e.target.dataset.id;
            const name = e.target.dataset.name;
            quickEditName(id, name);
        });
    });

    document.querySelectorAll('.edit-customer-btn').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation();
            const id = e.target.dataset.id;
            editCustomer(id);
        });
    });

    document.querySelectorAll('.delete-customer-btn').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation();
            const id = e.target.dataset.id;
            confirmDelete(id);
        });
    });
}

// Open modal for adding new customer
function openModal(customer = null) {
    if (customer) {
        modalTitle.textContent = 'Edit Customer';
        populateForm(customer);
        console.log('Opening edit modal for customer:', customer.id);
    } else {
        modalTitle.textContent = 'Add New Customer';
        customerForm.reset();
        document.getElementById('customerId').value = '';
        document.getElementById('createdDate').value = '';
        console.log('Opening add modal');
    }
    customerModal.classList.remove('hidden');
}

// Close modal
function closeModal() {
    customerModal.classList.add('hidden');
    customerForm.reset();
}

// Populate form with customer data
function populateForm(customer) {
    document.getElementById('customerId').value = customer.id;
    document.getElementById('name').value = customer.name;
    document.getElementById('age').value = customer.age;
    document.getElementById('country').value = customer.country;
    document.getElementById('revenue').value = customer.revenue;
    document.getElementById('isActive').checked = customer.isActive;
    document.getElementById('tags').value = customer.tags ? customer.tags.join(', ') : '';
    document.getElementById('createdDate').value = customer.createdDate;
}

// Handle form submission
async function handleFormSubmit(event) {
    event.preventDefault();
    const formData = new FormData(customerForm);
    const customerData = {
        name: formData.get('name'),
        age: parseInt(formData.get('age')),
        country: formData.get('country'),
        revenue: parseFloat(formData.get('revenue')),
        isActive: formData.get('isActive') === 'on',
        tags: formData.get('tags') ? formData.get('tags').split(',').map(tag => tag.trim()) : []
    };

    const customerId = formData.get('id');
    const createdDate = formData.get('createdDate');

    // For PUT updates, we need to include the original createdDate
    if (customerId && createdDate) {
        customerData.createdDate = createdDate;
    }

    try {
        let response;
        if (customerId && customerId !== '') {
            // Update existing customer
            response = await fetch(`${API_BASE}/customers/${customerId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(customerData),
            });

            if (!response.ok) throw new Error('Failed to save customer');

            closeModal();
            // Update customer row in table without full reload
            updateCustomerInTable(customerId, customerData);
        } else {
            // Create new customer
            response = await fetch(`${API_BASE}/customers`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(customerData),
            });

            if (!response.ok) throw new Error('Failed to save customer');

            const customer = await response.json();
            closeModal();
            // Add new customer row to table without full reload
            addCustomerToTable(customer);
        }
    } catch (error) {
        console.error('Error saving customer:', error);
        alert('Failed to save customer. Please try again.');
    }
}

// Edit customer
async function editCustomer(id) {
    try {
        const response = await fetch(`${API_BASE}/customers/${id}`);
        if (!response.ok) throw new Error('Failed to load customer');
        const customer = await response.json();
        openModal(customer);
    } catch (error) {
        console.error('Error loading customer:', error);
        alert('Failed to load customer for editing.');
    }
}

// Confirm delete
function confirmDelete(id) {
    currentCustomerId = id;
    deleteModal.classList.remove('hidden');
}

// Close delete modal
function closeDeleteModal() {
    deleteModal.classList.add('hidden');
    currentCustomerId = null;
}

// Delete customer
async function deleteCustomer(id) {
    try {
        const response = await fetch(`${API_BASE}/customers/${id}`, {
            method: 'DELETE',
        });
        if (!response.ok) throw new Error('Failed to delete customer');

        closeDeleteModal();
        // Remove customer row from table without full reload
        removeCustomerFromTable(id);
    } catch (error) {
        console.error('Error deleting customer:', error);
        alert('Failed to delete customer. Please try again.');
    }
}

// Remove customer row from table without full reload
function removeCustomerFromTable(id) {
    const button = document.querySelector(`.delete-customer-btn[data-id="${id}"]`);
    if (button) {
        const row = button.closest('tr');
        if (row) {
            row.remove();
        }
    }
}

// Add new customer to table without full reload
function addCustomerToTable(customer) {
    const row = document.createElement('tr');
    row.innerHTML = `
        <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">${customer.id}</td>
        <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
            ${customer.name}
            <button class="ml-2 text-blue-600 hover:text-blue-900 text-xs edit-name-btn" data-id="${customer.id}" data-name="${customer.name}" title="Quick edit name" type="button">
                ✏️
            </button>
        </td>
        <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">${customer.age}</td>
        <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">${customer.country}</td>
        <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">$${customer.revenue.toFixed(2)}</td>
        <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
            <span class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${customer.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}">
                ${customer.isActive ? 'Active' : 'Inactive'}
            </span>
        </td>
        <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">${customer.tags ? customer.tags.join(', ') : ''}</td>
        <td class="px-6 py-4 whitespace-nowrap text-sm font-medium">
            <button class="text-indigo-600 hover:text-indigo-900 mr-2 edit-customer-btn" data-id="${customer.id}" type="button">Edit</button>
            <button class="text-red-600 hover:text-red-900 delete-customer-btn" data-id="${customer.id}" type="button">Delete</button>
        </td>
    `;
    customerTableBody.appendChild(row);

    // Add event listeners to new buttons
    addEventListenersToRow(row);
}

// Update existing customer in table without full reload
function updateCustomerInTable(customerId, customerData) {
    const button = document.querySelector(`.edit-customer-btn[data-id="${customerId}"]`);
    if (button) {
        const row = button.closest('tr');
        if (row) {
            row.innerHTML = `
                <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">${customerId}</td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    ${customerData.name}
                    <button class="ml-2 text-blue-600 hover:text-blue-900 text-xs edit-name-btn" data-id="${customerId}" data-name="${customerData.name}" title="Quick edit name" type="button">
                        ✏️
                    </button>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">${customerData.age}</td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">${customerData.country}</td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">$${customerData.revenue.toFixed(2)}</td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    <span class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${customerData.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}">
                        ${customerData.isActive ? 'Active' : 'Inactive'}
                    </span>
                </td>
                <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-500">${customerData.tags ? customerData.tags.join(', ') : ''}</td>
                <td class="px-6 py-4 whitespace-nowrap text-sm font-medium">
                    <button class="text-indigo-600 hover:text-indigo-900 mr-2 edit-customer-btn" data-id="${customerId}" type="button">Edit</button>
                    <button class="text-red-600 hover:text-red-900 delete-customer-btn" data-id="${customerId}" type="button">Delete</button>
                </td>
            `;

            // Add event listeners to updated buttons
            addEventListenersToRow(row);
        }
    }
}

// Add event listeners to a specific row
function addEventListenersToRow(row) {
    const editNameBtn = row.querySelector('.edit-name-btn');
    const editCustomerBtn = row.querySelector('.edit-customer-btn');
    const deleteCustomerBtn = row.querySelector('.delete-customer-btn');

    if (editNameBtn) {
        editNameBtn.addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation();
            const id = e.target.dataset.id;
            const name = e.target.dataset.name;
            quickEditName(id, name);
        });
    }

    if (editCustomerBtn) {
        editCustomerBtn.addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation();
            const id = e.target.dataset.id;
            editCustomer(id);
        });
    }

    if (deleteCustomerBtn) {
        deleteCustomerBtn.addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation();
            const id = e.target.dataset.id;
            confirmDelete(id);
        });
    }
}

// Quick edit name using PATCH
async function quickEditName(id, currentName) {
    currentQuickEditId = id;
    document.getElementById('quickEditName').value = currentName;
    quickEditModal.classList.remove('hidden');

    // Focus and select text in the input field
    setTimeout(() => {
        const nameInput = document.getElementById('quickEditName');
        nameInput.focus();
        nameInput.select();
    }, 100);
}

// Close quick edit modal
function closeQuickEditModal() {
    quickEditModal.classList.add('hidden');
    currentQuickEditId = null;
    quickEditForm.reset();
}

// Handle quick edit form submission
async function handleQuickEditSubmit(event) {
    event.preventDefault();
    const newName = document.getElementById('quickEditName').value.trim();

    if (newName && currentQuickEditId) {
        try {
            const response = await fetch(`${API_BASE}/customers/${currentQuickEditId}`, {
                method: 'PATCH',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ name: newName }),
            });

            if (!response.ok) throw new Error('Failed to update customer name');

            closeQuickEditModal();
            // Update just the name in the table without full reload
            updateCustomerNameInTable(currentQuickEditId, newName);
        } catch (error) {
            console.error('Error updating customer name:', error);
            alert('Failed to update customer name. Please try again.');
        }
    }
}

// Update customer name in table without full reload
function updateCustomerNameInTable(id, newName) {
    const button = document.querySelector(`.edit-name-btn[data-id="${id}"]`);
    if (button) {
        button.dataset.name = newName;
        const nameCell = button.parentElement;
        nameCell.innerHTML = `${newName}<button class="ml-2 text-blue-600 hover:text-blue-900 text-xs edit-name-btn" data-id="${id}" data-name="${newName}" title="Quick edit name" type="button">✏️</button>`;

        // Re-add event listener to the new button
        const newButton = nameCell.querySelector('.edit-name-btn');
        newButton.addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation();
            const buttonId = e.target.dataset.id;
            const buttonName = e.target.dataset.name;
            quickEditName(buttonId, buttonName);
        });
    }
}