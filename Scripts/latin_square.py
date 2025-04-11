import itertools
import random
from collections import defaultdict

def generate_all_unique_setups():
    machines_list = list(itertools.permutations(['A', 'B', 'C']))  # 6
    mode_perms = list(itertools.permutations([1, 2, 3]))           # 6

    unique_mode_triplets = list(itertools.combinations(mode_perms, 3))  # 20

    all_participant_setups = []
    participant_id = 1

    for machine_order in machines_list:                     # 6
        for mode_triplet in unique_mode_triplets:           # 20
            for mode_ordering in itertools.permutations(mode_triplet):  # 6
                trials = []
                for machine, mode_seq in zip(machine_order, mode_ordering):
                    trials.append({
                        "machine": machine,
                        "modes": list(mode_seq)
                    })
                all_participant_setups.append({
                    "participant": participant_id,
                    "trials": trials
                })
                participant_id += 1

    return all_participant_setups

def stratified_sampling(participants, num_samples_per_group):
    machine_order_groups = defaultdict(list)
    for p in participants:
        machine_order_signature = tuple(trial['machine'] for trial in p['trials'])
        machine_order_groups[machine_order_signature].append(p)
    
    # Stratified sample from each group
    stratified_participants = []
    for machine_order, group in machine_order_groups.items():
        sampled_group = random.sample(group, num_samples_per_group)
        stratified_participants.extend(sampled_group)

    return stratified_participants

def print_sample(participants):
    for p in participants:
        print(f"\nParticipant {p['participant']}:")
        for i, trial in enumerate(p['trials'], 1):
            print(f"  Trial {i}: Machine {trial['machine']}, Modes {trial['modes']}")


participants = generate_all_unique_setups()


stratified_participants = stratified_sampling(participants, 1)

print(f"\nStratified Sample of {len(stratified_participants)} participants:")
print_sample(stratified_participants)
