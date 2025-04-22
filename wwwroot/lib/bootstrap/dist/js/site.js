document.addEventListener("DOMContentLoaded", function () {
  const btnShowSearchBar = document.getElementById("btnShowSearchBar");
  const btnCloseSearchBar = document.getElementById("btnCloseSearchBar");
  const navbarList = document.getElementById("navbarList");
  const navbarSearch = document.getElementById("navbarSearch");

  console.log("✅ JavaScript is running...");

  if (btnShowSearchBar && btnCloseSearchBar) {
      btnShowSearchBar.addEventListener("click", function () {
          navbarList?.classList.add("d-none");
          navbarSearch?.classList.remove("d-none");
          btnShowSearchBar.classList.add("d-none");
          btnCloseSearchBar.classList.remove("d-none");
      });

      btnCloseSearchBar.addEventListener("click", function () {
          navbarList?.classList.remove("d-none");
          navbarSearch?.classList.add("d-none");
          btnShowSearchBar.classList.remove("d-none");
          btnCloseSearchBar.classList.add("d-none");
      });
  } else {
      console.warn("Buttons not found in DOM");
  }
});
// Initialize filters based on URL parameters
document.addEventListener('DOMContentLoaded', function() {
    const urlParams = new URLSearchParams(window.location.search);
    
    // Set category checkboxes
    const categories = urlParams.get('category')?.split(',') || [];
    categories.forEach(categoryId => {
        const checkbox = document.querySelector(`input[name="category"][value="${categoryId}"]`);
        if (checkbox) checkbox.checked = true;
    });

    // Set price range checkboxes
    const prices = urlParams.get('price')?.split(',') || [];
    prices.forEach(price => {
        const checkbox = document.querySelector(`input[name="price"][value="${price}"]`);
        if (checkbox) checkbox.checked = true;
    });

    // Set color checkboxes
    const colors = urlParams.get('color')?.split(',') || [];
    colors.forEach(color => {
        const checkbox = document.querySelector(`input[name="color"][value="${color}"]`);
        if (checkbox) checkbox.checked = true;
    });

    // Set stock status checkboxes
    const stockStatus = urlParams.get('stock')?.split(',') || [];
    stockStatus.forEach(status => {
        const checkbox = document.querySelector(`input[name="stock"][value="${status}"]`);
        if (checkbox) checkbox.checked = true;
    });
});
function addToCart(productId) {
    fetch('/Cart/AddToCart', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            productId: productId,
            quantity: 1
        })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            alert('Product added to cart successfully!');
        } else {
            alert(data.message);
        }
    });
}

// Cart functionality
$(document).ready(function () {
    function updateQuantity(productId, quantity) {
        $.ajax({
            url: '/Cart/UpdateQuantity',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ productId: productId, quantity: quantity }),
            success: function(response) {
                if (response.success) {
                    // Update the total price if available
                    if (response.newTotal) {
                        $('tfoot td:last').prev().html('<strong>' + response.newTotal + ' ₫</strong>');
                    }
                    // Show success message if available
                    if (response.message) {
                        alert(response.message);
                    }
                } else {
                    // Show the specific error message from the server
                    alert(response.message || 'Failed to update quantity');
                }
            },
            error: function(xhr, status, error) {
                console.error('Error:', error);
                alert('Failed to update quantity. Please try again later.');
            }
        });
    }

    // Decrease quantity button
    $(document).on('click', '.decrease-quantity', function () {
        var row = $(this).closest('tr');
        var input = row.find('.quantity-input');
        var currentValue = parseInt(input.val());
        if (currentValue > 1) {
            updateQuantity(row.data('product-id'), currentValue - 1);
        }
    });

    // Increase quantity button
    $(document).on('click', '.increase-quantity', function () {
        var row = $(this).closest('tr');
        var input = row.find('.quantity-input');
        var currentValue = parseInt(input.val());
        var maxValue = parseInt(input.attr('max'));
        if (currentValue < maxValue) {
            updateQuantity(row.data('product-id'), currentValue + 1);
        }
    });

    // Quantity input change
    $(document).on('change', '.quantity-input', function () {
        var row = $(this).closest('tr');
        var value = parseInt($(this).val());
        var max = parseInt($(this).attr('max'));
        var min = parseInt($(this).attr('min'));

        if (isNaN(value) || value < min) {
            value = min;
        } else if (value > max) {
            value = max;
        }

        $(this).val(value);
        updateQuantity(row.data('product-id'), value);
    });

    // Remove item button
    $(document).on('click', '.remove-item', function () {
        var row = $(this).closest('tr');
        if (confirm('Are you sure you want to remove this item from your cart?')) {
            $.post('/Cart/RemoveItem', { productId: row.data('product-id') })
                .done(function (response) {
                    if (response.success) {
                        location.reload();
                    } else {
                        alert(response.message || 'An error occurred while removing the item.');
                    }
                })
                .fail(function () {
                    alert('An error occurred while removing the item.');
                });
        }
    });

    // Clear cart button
    $(document).on('click', '#clearCart', function () {
        if (confirm('Are you sure you want to clear your entire cart?')) {
            $.post('/Cart/ClearCart')
                .done(function (response) {
                    if (response.success) {
                        location.reload();
                    } else {
                        alert(response.message || 'An error occurred while clearing the cart.');
                    }
                })
                .fail(function () {
                    alert('An error occurred while clearing the cart.');
                });
        }
    });

    // Checkout button
    $(document).on('click', '#checkout', function () {
        $.post('/Cart/Checkout')
            .done(function (response) {
                if (response.success) {
                    window.location.href = '/Orders/Details/' + response.orderId;
                } else {
                    alert(response.message || 'An error occurred during checkout.');
                }
            })
            .fail(function () {
                alert('An error occurred during checkout.');
            });
    });
});
function clearAllFilters() {
    document.querySelectorAll('input[type="checkbox"]').forEach(input => {
        input.checked = false;
    });
    applyFilters(); // Apply changes immediately after clearing
}

function applyFilters() {
    const urlParams = new URLSearchParams(window.location.search);
    
    // Clear existing filter parameters
    ['category', 'price', 'color', 'stock'].forEach(param => {
        urlParams.delete(param);
    });

    // Get selected categories
    const selectedCategories = Array.from(document.querySelectorAll('input[name="category"]:checked'))
        .map(input => input.value);
    if (selectedCategories.length > 0) {
        urlParams.set('category', selectedCategories.join(','));
    }

    // Get selected price ranges
    const selectedPrices = Array.from(document.querySelectorAll('input[name="price"]:checked'))
        .map(input => input.value);
    if (selectedPrices.length > 0) {
        urlParams.set('price', selectedPrices.join(','));
    }

    // Get selected colors
    const selectedColors = Array.from(document.querySelectorAll('input[name="color"]:checked'))
        .map(input => input.value);
    if (selectedColors.length > 0) {
        urlParams.set('color', selectedColors.join(','));
    }

    // Get selected stock status
    const selectedStock = Array.from(document.querySelectorAll('input[name="stock"]:checked'))
        .map(input => input.value);
    if (selectedStock.length > 0) {
        urlParams.set('stock', selectedStock.join(','));
    }

    // Update URL with new parameters
    window.location.search = urlParams.toString();
}
