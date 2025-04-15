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
