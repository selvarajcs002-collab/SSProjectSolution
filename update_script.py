import re

with open('sp_meter.sql', 'r', encoding='utf-16le') as f:
    content = f.read()

# Replace CREATE PROCEDURE with ALTER PROCEDURE
content = re.sub(r'CREATE PROCEDURE', 'ALTER PROCEDURE', content, flags=re.IGNORECASE)

# Add @InwardDate DATETIME = NULL
content = re.sub(r'(@MeterDetails\s+MeterDetailType\s+READONLY)', r'\1,\n    @InwardDate DATETIME = NULL', content)

# In UPDATE Inward, add InwardDate = ISNULL(@InwardDate, InwardDate)
# Make sure we don't change UpdatedDate which is already GETDATE()
content = re.sub(r'(UpdatedDate\s*=\s*GETDATE\(\))', r'\1,\n                InwardDate = ISNULL(@InwardDate, InwardDate)', content)

# In INSERT INTO Inward (..., CreatedBy, CreatedDate) VALUES (..., @CreatedBy, GETDATE())
content = re.sub(r'IMD_CREATED_DATE', r'IMD_CREATED_DATE, InwardDate', content)
content = re.sub(r'GETDATE\(\)\r?\n\s*FROM @MeterDetails', r'GETDATE(), @InwardDate\n            FROM @MeterDetails', content)

content = re.sub(r'CreatedDate', r'CreatedDate, InwardDate', content)
# It's tricky to regex this blindly, so I'll write the script that just does a simple replace on the known structure:
# The INSERT INTO Inward doesn't exist in SP_SAVE_INWARD_METER! It only inserts into INWARD_METER_DETAIL, but wait!
# SP_SAVE_INWARD_METER does insert into Inward if @InwardId = 0!

with open('update_sp_meter.sql', 'w', encoding='utf-8') as f:
    f.write(content)
