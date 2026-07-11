import re

def update_sp(filename, out_filename):
    with open(filename, 'r', encoding='utf-8') as f:
        content = f.read()

    # Replace CREATE with ALTER
    content = re.sub(r'CREATE\s+PROCEDURE', 'ALTER PROCEDURE', content, flags=re.IGNORECASE)

    # We need to replace I.CreatedDate, with COALESCE(I.InwardDate, I.CreatedDate) AS CreatedDate,
    # but ONLY in the SELECT list, not in the WHERE or ORDER BY.
    # The select list typically looks like:
    # I.CreatedBy,
    # I.CreatedDate,
    # I.UpdatedDate,
    # Let's target exactly `I.CreatedDate,`
    
    content = re.sub(r'(\s+)I\.CreatedDate,', r'\1COALESCE(I.InwardDate, I.CreatedDate) AS CreatedDate,', content)

    # For Outward we can leave O.CreatedDate, or if we want we can do O.OutwardDate if it exists. 
    # But since Outward doesn't have OutwardDate yet (we didn't add it in this task), we just leave O.CreatedDate.
    # We should make sure we didn't touch O.CreatedDate, the regex targets I.CreatedDate,

    with open(out_filename, 'w', encoding='utf-8') as f:
        f.write(content)

update_sp('sp_inward_outward_utf8.sql', 'update_inward_outward_filter.sql')
update_sp('sp_inward_outward_all_utf8.sql', 'update_inward_outward_all.sql')
