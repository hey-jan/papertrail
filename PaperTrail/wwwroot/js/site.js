$(document).ready(function () {
    const searchInput = $('.search-input');
    const suggestionsContainer = $('#search-suggestions');

    searchInput.on('input', function () {
        const term = $(this).val();
        if (term.length < 2) {
            suggestionsContainer.addClass('d-none');
            return;
        }

        $.get('/Home/SearchSuggestions', { term: term }, function (data) {
            if (data && data.length > 0) {
                suggestionsContainer.empty();
                data.forEach(item => {
                    suggestionsContainer.append(`
                        <a href="/Home/Details/${item.id}" class="list-group-item list-group-item-action border-0 py-2">
                            <div class="fw-bold">${item.title}</div>
                            <small class="text-muted">${item.author}</small>
                        </a>
                    `);
                });
                suggestionsContainer.removeClass('d-none');
            } else {
                suggestionsContainer.addClass('d-none');
            }
        });
    });

    $(document).on('click', function (e) {
        if (!$(e.target).closest('.search-container').length) {
            suggestionsContainer.addClass('d-none');
        }
    });
});
