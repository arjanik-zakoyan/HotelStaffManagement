// Search User
document.addEventListener("DOMContentLoaded", () => {
    const searchInput = document.getElementById("managerSearch");
    const tableRows = document.querySelectorAll("#managerTable tbody tr");

    if (searchInput) {
        searchInput.addEventListener("input", function () {
            const keyword = this.value.toLowerCase();

            tableRows.forEach(row => {
                const name = row.children[0].innerText.toLowerCase();
                const username = row.children[1].innerText.toLowerCase();

                row.style.display = (name.includes(keyword) || username.includes(keyword)) ? "" : "none";
            });
        });
    }
});
